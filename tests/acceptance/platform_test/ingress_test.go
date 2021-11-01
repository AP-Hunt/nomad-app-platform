package platform_test

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net/http"
	"net/url"
	"strings"
	"time"

	"github.com/hashicorp/nomad/api"
	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"
)

var (
	nomadApi *api.Client
	jobId    string
)

func allTasksAreRunning(nomadApi *api.Client, evalID string) bool {
	allocations, _, err := nomadApi.Evaluations().Allocations(evalID, &api.QueryOptions{})
	Expect(err).ToNot(HaveOccurred())

	for _, alloc := range allocations {
		if alloc.ClientStatus != "running" {
			return false
		}
	}

	return true
}

type probeResponse struct {
	Node  string `json:"node"`
	Index string `json:"index"`
}

var errNotFound = fmt.Errorf("not found")

func probeRequest() (*probeResponse, error) {
	client := http.Client{}
	req := http.Request{}
	req.Method = "GET"
	req.URL = &url.URL{Scheme: "http", Host: "paas.dev"}
	req.Host = "ingress-probe.paas.dev"

	resp, err := client.Do(&req)
	if err != nil {
		return nil, err
	}

	if resp.StatusCode == http.StatusNotFound {
		return nil, errNotFound
	}

	content, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return nil, err
	}

	response := probeResponse{}
	err = json.Unmarshal(content, &response)
	if err != nil {
		return nil, err
	}

	return &response, nil
}

var _ = Describe("Ingress", func() {

	BeforeEach(func() {
		apiConfig := api.Config{
			Address: "http://paas.dev:4646",
		}
		apiClient, err := api.NewClient(&apiConfig)
		Expect(err).ToNot(HaveOccurred())

		nomadApi = apiClient

		jobSpecBytes, err := ioutil.ReadFile("testdata/ingress-probe.hcl")
		Expect(err).ToNot(HaveOccurred())

		job, err := nomadApi.Jobs().ParseHCL(string(jobSpecBytes), true)
		Expect(err).ToNot(HaveOccurred())

		result, _, err := nomadApi.Jobs().Register(job, &api.WriteOptions{})
		Expect(err).ToNot(HaveOccurred())

		jobId = *job.ID

		Eventually(allTasksAreRunning(nomadApi, result.EvalID), 3*time.Minute, 2*time.Second).
			Should(BeTrue())

	})

	AfterEach(func() {
		_, _, err := nomadApi.Jobs().Deregister(jobId, true, &api.WriteOptions{})
		Expect(err).ToNot(HaveOccurred())
	})

	It("can load balances across all application instances", func() {
		// Make an HTTP request for the probe
		// repeatedly, collecting the unique
		// instances which come back.
		instances := map[string]bool{}
		Eventually(func() []string {
			response, err := probeRequest()

			if err == errNotFound {
				return []string{}
			}
			Expect(err).ToNot(HaveOccurred())
			instances[response.Index] = true

			names := []string{}
			for n, _ := range instances {
				names = append(names, n)
			}

			return names
		}, 2*time.Minute, 1*time.Second).Should(ConsistOf("0", "1", "2"))
	})

	It("load balances across all nodes", func() {
		By("assuming that the application is present on all nodes")

		// Make an HTTP request for the probe
		// repeatedly, collecting the unique
		// node names which come back.
		nodes := map[string]bool{}
		Eventually(func() []string {
			response, err := probeRequest()
			if err == errNotFound {
				return []string{}
			}
			Expect(err).ToNot(HaveOccurred())

			parts := strings.Split(response.Node, "-")
			ipComponent := strings.Join(parts[1:], ".")

			nodes[ipComponent] = true

			names := []string{}
			for n, _ := range nodes {
				names = append(names, n)
			}

			return names
		}, 2*time.Minute, 1*time.Second).Should(ConsistOf(
			"192.168.33.10",
			"192.168.33.11",
			"192.168.33.12",
		))
	})
})

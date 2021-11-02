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

func allTasksAreRunning(nomadApi *api.Client, evalID string) func(Gomega) bool {
	return func(g Gomega) bool {
		allocations, _, err := nomadApi.Evaluations().Allocations(evalID, &api.QueryOptions{})
		g.Expect(err).ToNot(HaveOccurred())

		for _, alloc := range allocations {
			if alloc.ClientStatus != "running" {
				return false
			}
		}

		return true
	}
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

type nodesAndInstances struct {
	nodes     map[string]bool
	instances map[string]bool
}

func (n *nodesAndInstances) AddNode(node string) {
	parts := strings.Split(node, "-")
	ipComponent := strings.Join(parts[1:], ".")
	n.nodes[ipComponent] = true
}

func (n *nodesAndInstances) GetNodes() []string {
	nodes := make([]string, len(n.nodes))

	i := 0
	for k, _ := range n.nodes {
		nodes[i] = k
		i = i + 1
	}

	return nodes
}

func (n *nodesAndInstances) AddInstance(instance string) {
	n.instances[instance] = true
}

func (n *nodesAndInstances) GetInstances() []string {
	instances := make([]string, len(n.instances))

	i := 0
	for k, _ := range n.instances {
		instances[i] = k
		i = i + 1
	}

	return instances
}

var _ = Describe("Ingress", func() {
	var (
		nomadApi *api.Client
		jobId    string
	)

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

	It("can load balances across all application instances and across all nodes", func() {
		By("assuming that the application is present on all nodes")

		result := nodesAndInstances{
			nodes:     map[string]bool{},
			instances: map[string]bool{},
		}
		Eventually(func(g Gomega) int {
			response, err := probeRequest()

			if err != nil {
				g.Expect(err).To(MatchError(errNotFound))
				return 0
			}

			result.AddInstance(response.Index)
			result.AddNode(response.Node)
			g.Expect(result.GetInstances()).To(ConsistOf("0", "1", "2"))
			g.Expect(result.GetNodes()).To(ConsistOf(
				"192.168.33.10",
				"192.168.33.11",
				"192.168.33.12",
			))

			return len(result.GetNodes())
		}, 5*time.Minute, 1*time.Second).Should(Equal(3))
	})
})

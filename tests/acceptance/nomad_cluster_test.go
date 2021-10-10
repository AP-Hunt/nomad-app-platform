package acceptance_test

import (
	"fmt"

	"github.com/hashicorp/nomad/api"
	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"
)

var _ = Describe("["+HostName()+"] Nomad Cluster", func() {
	Describe("Members", func() {
		It("There should be 2 nodes", func() {
			apiConfig := api.Config{
				Address: fmt.Sprintf("http://%s:4646", IpAddress),
			}
			apiClient, err := api.NewClient(&apiConfig)
			Expect(err).ToNot(HaveOccurred())

			nodes, _, err := apiClient.Nodes().List(nil)
			Expect(err).ToNot(HaveOccurred())

			Expect(len(nodes)).To(Equal(2))
		})

		It("There should be 1 server", func() {
			apiConfig := api.Config{
				Address: fmt.Sprintf("http://%s:4646", IpAddress),
			}
			apiClient, err := api.NewClient(&apiConfig)
			Expect(err).ToNot(HaveOccurred())

			serverMembers, err := apiClient.Agent().Members()
			Expect(err).ToNot(HaveOccurred())
			Expect(len(serverMembers.Members)).To(Equal(1))
		})
	})
})

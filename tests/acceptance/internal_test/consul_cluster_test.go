package internal_test

import (
	"fmt"

	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"

	"github.com/hashicorp/consul/api"
)

var _ = Describe("["+HostName()+"] Consul Cluster", func() {
	Describe("Members", func() {
		It("There should be 3 members", func() {
			consulConfig := api.DefaultConfig()
			consulConfig.Address = fmt.Sprintf("http://%s:8500", "127.0.0.1")

			client, err := api.NewClient(consulConfig)
			Expect(err).ToNot(HaveOccurred())

			nodes, _, err := client.Catalog().Nodes(nil)
			Expect(err).ToNot(HaveOccurred())

			Expect(len(nodes)).To(Equal(3))
		})
	})
})

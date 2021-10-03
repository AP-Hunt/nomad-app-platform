package acceptance_test

import (
	"context"
	"fmt"

	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"
	etcdclient "go.etcd.io/etcd/client/v3"
)

var _ = Describe("Etcd", func() {
	Describe("Members", func() {
		It("There should be 3 members", func() {
			// consulConfig := api.DefaultConfig()
			// apiConfig := api.Config{
			// 	Address:    fmt.Sprintf("http://%s:4646", IpAddress),
			// }
			// consulConfig.Address = fmt.Sprintf("http://%s:8500", "127.0.0.1")

			client, err := etcdclient.New(etcdclient.Config{
				Endpoints: []string{fmt.Sprintf("http://%s:2380", IpAddress)},
			})
			Expect(err).ToNot(HaveOccurred())
			defer client.Close()

			resp, err := client.MemberList(context.Background())
			Expect(err).ToNot(HaveOccurred())

			Expect(len(resp.Members)).To(Equal(3))
		})
	})
})

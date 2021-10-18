package internal_test

import (
	"context"

	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"

	"github.com/docker/docker/api/types"
	"github.com/docker/docker/client"
)

var _ = Describe("["+HostName()+"] Calico", func() {
	Describe("Readiness", func() {
		It("Is ready", func() {
			dockerClient, err := client.NewClientWithOpts(client.FromEnv)
			Expect(err).ToNot(HaveOccurred())

			exec, err := dockerClient.ContainerExecCreate(context.Background(), "calico-node", types.ExecConfig{
				Cmd: []string{"/bin/calico-node", "-felix-ready"},
			})
			Expect(err).ToNot(HaveOccurred())

			err = dockerClient.ContainerExecStart(context.Background(), exec.ID, types.ExecStartCheck{})
			Expect(err).ToNot(HaveOccurred())

			result, err := dockerClient.ContainerExecInspect(context.Background(), exec.ID)
			Expect(err).ToNot(HaveOccurred())

			Expect(result.ExitCode).To(Equal(0), "Ran `/bin/calico-node -felix-ready` inside the `calico-node` container and it did not exit with code 0. See https://docs.projectcalico.org/reference/node/configuration#node-readiness")
		})
	})
})

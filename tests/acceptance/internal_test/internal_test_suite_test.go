package internal_test

import (
	"os"
	"testing"

	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"
)

var (
	IpAddress string
)

func HostName() string {
	name, err := os.Hostname()

	if err != nil {
		panic(err)
	}

	return name
}
func TestInternalTest(t *testing.T) {
	BeforeSuite(func() {
		IpAddress = os.Getenv("NODE_IP")
	})

	RegisterFailHandler(Fail)
	RunSpecs(t, "Internal Test Suite (run from inside a node)")
}

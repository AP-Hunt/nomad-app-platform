package acceptance_test

import (
	"os"
	"testing"

	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"
)

var (
	IpAddress string
)

func TestAcceptance(t *testing.T) {
	BeforeSuite(func(){
		IpAddress = os.Getenv("NODE_IP")
	})


	RegisterFailHandler(Fail)
	RunSpecs(t, "Acceptance Suite")
}


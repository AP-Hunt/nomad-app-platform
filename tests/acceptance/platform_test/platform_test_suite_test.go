package platform_test

import (
	"testing"

	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"
)

func TestExternalTest(t *testing.T) {
	RegisterFailHandler(Fail)
	RunSpecs(t, "Platform Test Suite")
}

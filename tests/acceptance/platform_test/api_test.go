package platform_test

import (
	"archive/zip"
	"encoding/json"
	"io"
	"io/ioutil"
	"path"
	"time"

	. "github.com/onsi/ginkgo"
	. "github.com/onsi/gomega"

	"bytes"
	"mime/multipart"
	"net/http"
	"net/url"
	"os"
)

func createZipArchive(sourceFilesPath string, fileName string) (*os.File, error) {
	workingDir, err := os.Getwd()
	if err != nil {
		return nil, err
	}

	archivePath := path.Join(workingDir, "testdata", fileName)
	archive, err := os.Create(archivePath)
	if err != nil {
		return nil, err
	}
	defer archive.Close()

	zipWriter := zip.NewWriter(archive)

	dirContent, err := ioutil.ReadDir(sourceFilesPath)
	if err != nil {
		return nil, err
	}

	for _, fileInfo := range dirContent {
		filePath := path.Join(sourceFilesPath, fileInfo.Name())

		fileStream, err := os.Open(filePath)
		if err != nil {
			return nil, err
		}
		defer fileStream.Close()

		zipEntry, err := zipWriter.Create(fileInfo.Name())
		if err != nil {
			return nil, err
		}

		_, err = io.Copy(zipEntry, fileStream)
		if err != nil {
			return nil, err
		}
	}

	zipWriter.Close()

	zipFileStream, err := os.Open(archivePath)
	if err != nil {
		return nil, err
	}

	return zipFileStream, nil
}

func createMultipartFormBody(archive *os.File) (*bytes.Buffer, string, error) {
	var buffer bytes.Buffer
	multipartWriter := multipart.NewWriter(&buffer)
	defer multipartWriter.Close()

	fileWriter, err := multipartWriter.CreateFormFile("source_bundle", archive.Name())
	if err != nil {
		return nil, "", err
	}

	_, err = io.Copy(fileWriter, archive)
	if err != nil {
		return nil, "", err
	}

	return &buffer, multipartWriter.FormDataContentType(), nil
}

type AppResponse struct {
	Id      string
	Name    string
	Version int
}

var _ = Describe("Api", func() {
	It("Accepts a zip file containing the source, and runs the application on the platform", func() {
		sourceArchive, err := createZipArchive("testdata/simple_go_app", "go-app.zip")
		Expect(err).ToNot(HaveOccurred())
		defer sourceArchive.Close()

		body, contentType, err := createMultipartFormBody(sourceArchive)
		Expect(err).ToNot(HaveOccurred())

		httpClient := http.Client{}

		req, err := http.NewRequest("POST", "http://api.paas.dev/apps/", body)
		Expect(err).ToNot(HaveOccurred())

		req.Header.Set("Content-Type", contentType)

		res, err := httpClient.Do(req)
		Expect(err).ToNot(HaveOccurred())
		Expect(res.StatusCode).To(Equal(http.StatusAccepted))

		appResponse := AppResponse{}
		respBytes, err := ioutil.ReadAll(res.Body)
		Expect(err).ToNot(HaveOccurred())

		err = json.Unmarshal(respBytes, &appResponse)
		Expect(err).ToNot(HaveOccurred())

		Eventually(func() int {
			client := http.Client{}
			req := http.Request{}
			req.Method = "GET"
			req.URL = &url.URL{Scheme: "http", Host: "paas.dev"}
			req.Host = "go-app.paas.dev"

			resp, err := client.Do(&req)
			if err != nil {
				return -1
			}

			return resp.StatusCode
		}, 15*time.Minute, 10*time.Second).Should(Equal(200))
	})
})

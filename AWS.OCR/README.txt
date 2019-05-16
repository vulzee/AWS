# Save image to file
docker save -o <path for generated tar file> <image name>
example: docker save -o c:/myfile.tar centos:16

# Load image from file
docker load -i <path to image tar file>
example: docker load -i c:/myfile.tar
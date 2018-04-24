BASE_PATH=$(cd `dirname $0`; pwd)
BUILD_PATH=${BASE_PATH}/build

APPLICATION_OUTPUT_FILE=${BUILD_PATH}/Application/appSrv


# check require
# cmake 2.6 or above
# gcc 4.8 or above

sudo rm -rf ${BUILD_PATH}
mkdir ${BUILD_PATH}
cmake ..
make -j 4







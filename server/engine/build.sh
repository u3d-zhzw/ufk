#!bin/sh

BASE_PATH=$(cd `dirname $0`; pwd)
BUILD_PATH=${BASE_PATH}/build

LIB_OUTPUT_PATH=${BUILD_PATH}/lib
DEPLOY_PATH=${BASE_PATH}/deploy

# require
# cmake 2.6 or above
# gcc 4.8 or above

rm -rf ${BUILD_PATH}
mkdir ${BUILD_PATH}
cd ${BUILD_PATH}
cmake ..
make -j 4

# TODO: check directionary existed
# TODO: copy list
APPSRV_OUTPUT=${BUILD_PATH}/Application/appSrv
cp -vf ${APPSRV_OUTPUT} ${DEPLOY_PATH}/servers/bin/
#cd ${LIB_OUTPUT_PATH}/* ${DEPLOY_PATH}/lib







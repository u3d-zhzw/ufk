#!bin/sh

BASE_PATH=$(cd `dirname $0`; pwd)
BUILD_PATH=${BASE_PATH}/build

LIB_OUTPUT_PATH=${BUILD_PATH}/lib
DEPLOY_PATH=${BASE_PATH}/deploy

# require
# cmake 2.6 or above
# gcc 4.8 or above

function copy
{
    # TODO: check directionary existed
    # TODO: copy list
    APPSRV_OUTPUT=${BUILD_PATH}/application/appSrv
    cp -vf ${APPSRV_OUTPUT} ${DEPLOY_PATH}/servers/bin/

#    cp ${LIB_OUTPUT_PATH}/* ${DEPLOY_PATH}/lib

    # todo: auto make test dir
    cp -vf ${BUILD_PATH}/test/TestNetPacket/testNetPacket ${DEPLOY_PATH}/test/

}

function build
{
    cd ${BUILD_PATH}
    make -j 4
}

function clean_build
{
    rm -rf ${BUILD_PATH}
    mkdir ${BUILD_PATH}
    cd ${BUILD_PATH}
    cmake ..
    make -j 4
}

# 操作
act=$1

case $act in 
    b)
        build
        copy
        ;;

    cb)
        clean_build
        copy
        ;;

    *)
        ;;
esac






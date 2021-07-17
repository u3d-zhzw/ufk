#!/bin/bash

BASE_PATH=$(cd `dirname $0`; pwd)

BUILD_PATH=$BASE_PATH/build
rm -rf $BUILD_PATH
mkdir $BUILD_PATH

buildluajit()
{
    LUAJIT_BUILD_PATH=$BUILD_PATH/LuaJIT-2.0.5_build
    rm -rf $LUAJIT_BUILD_PATH
    mkdir $LUAJIT_BUILD_PATH

    cd LuaJIT-2.0.5
    make PREFIX=$LUAJIT_BUILD_PATH && sudo make install PREFIX=$LUAJIT_BUILD_PATH

    ENGINE_PATH_INCLUDE_PATH=$BASE_PATH/../engine/3rdParty/include
    cp -r $LUAJIT_BUILD_PATH/include/luajit-2.0 $ENGINE_PATH_INCLUDE_PATH/luajit

    ENGINE_PATH_LIB_PATH=$BASE_PATH/../engine/3rdParty/lib
    cp $LUAJIT_BUILD_PATH/lib/libluajit-5.1.a $ENGINE_PATH_LIB_PATH/libluajit.a
}

buildluajit

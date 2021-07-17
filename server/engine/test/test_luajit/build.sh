#!/bin/bash

BASE_PATH=$(cd `dirname $0`; pwd)

rm -rf build
mkdir build
cd build
cmake .. && make -j 4

#THIRD_PARTY_LIB_PATH=../../3rdParty/lib
# echo $THIRD_PARTY_LIB_PATH
# export PATH=$THIRD_PARTY_LIB_PATH:$PATH
./test_luajit


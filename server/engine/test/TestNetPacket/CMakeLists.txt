cmake_minimum_required(VERSION 2.6)

PROJECT(testNetPacket)

SET(PROJ_ROOT ${PROJECT_SOURCE_DIR}/../..)

FILE(GLOB_RECURSE SRC ${PROJECT_SOURCE_DIR}/*.cpp ${PROJECT_SOURCE_DIR}/*.c ${PROJ_ROOT}/application/Application.cpp)

SET(CMAKE_CXX_FLAGS " -fPIC -std=c++11 -g -w")
SET(CMAKE_C_FLAGS "-std=c++11 -g -w")

INCLUDE_DIRECTORIES(${PROJ_ROOT})
INCLUDE_DIRECTORIES(${PROJ_ROOT}/3rdParty/include)
LINK_DIRECTORIES(${PROJ_ROOT}/3rdParty/lib)

ADD_EXECUTABLE(testNetPacket ${SRC})

TARGET_LINK_LIBRARIES(testNetPacket network common event rt pb)

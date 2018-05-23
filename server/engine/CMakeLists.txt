cmake_minimum_required(VERSION 2.6)

PROJECT(UFK_SERVER)

# libs
ADD_SUBDIRECTORY(common)
ADD_SUBDIRECTORY(network)
ADD_SUBDIRECTORY(pb)

# excecute
ADD_SUBDIRECTORY(test)
ADD_SUBDIRECTORY(application)

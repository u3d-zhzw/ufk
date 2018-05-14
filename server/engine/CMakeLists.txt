cmake_minimum_required(VERSION 2.6)

PROJECT(UFK_SERVER)

# libs
ADD_SUBDIRECTORY(pb)
ADD_SUBDIRECTORY(network)

# excecute
ADD_SUBDIRECTORY(Application)

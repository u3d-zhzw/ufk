# !bin/sh

BASEPATH=$(cd `dirname $0`; pwd)

PROTO_BIN=$BASEPATH/../tools/protoc-3.4.0-linux-x86_64/bin/protoc

PROTO_SRC=./pb
CPP_SRC=./cpp

$PROTO_BIN -I=$PROTO_SRC --cpp_out=$CPP_SRC $PROTO_SRC/Person.proto

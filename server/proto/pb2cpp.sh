# !bin/sh

PROTO_BIN=./tools/protoc-3.4.0-linux-x86_64/bin/protoc

PROTO_SRC=./trunk/proto
CPP_SRC=./trunk/cpp

$PROTO_BIN -I=$PROTO_SRC --cpp_out=$CPP_SRC $PROTO_SRC/pkg_vs_pb.proto

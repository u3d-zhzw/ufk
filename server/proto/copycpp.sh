#!bin/sh
BASEPATH=$(cd `dirname $0`; pwd)

CPP_SRC=$BASEPATH/cpp
CPP_DST=$BASEPATH/../engine/pb

rm -rf $CPP_DST/*.h
rm -rf $CPP_DST/*.cc

cp $CPP_SRC/*.h $CPP_DST/
cp $CPP_SRC/*.cc $CPP_DST/

echo "done"

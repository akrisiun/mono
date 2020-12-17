
# brew install autoconf automake libtool pkg-config cmake python3

export PREFIX=/tmp/mono

mkdir $PREFIX
echo $PREFIX
./configure --prefix=$PREFIX --disable-nls --enable-wasm --with-xammac=yes

cd runtime/
make
make install

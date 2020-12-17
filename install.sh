
# brew install autoconf automake libtool pkg-config cmake python3

export PREFIX=/tmp/mono

mkdir $PREFIX
# Ensure you have write permissions to /usr/local
mkdir $PREFIX
sudo chown -R `whoami` $PREFIX

echo $PREFIX $CFLAGS

export CFLAGS="-arch x86_64 $CFLAGS"
export CCFLAGS="-arch x86_64 $CCFLAGS"
export CXXFLAGS="-arch x86_64 $CXXFLAGS"
export LDFLAGS="-arch x86_64 $LDFLAGS"

./configure --prefix=$PREFIX --disable-nls --enable-wasm --with-xammac=yes --host=x86_64-apple-darwin18.7.0

cd runtime/
make
make install

cd ../


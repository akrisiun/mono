MAKEFLAGS := $(MAKEFLAGS) --no-builtin-rules

DIST_SUBDIRS = x86 arm arm64 amd64 mips

AM_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir)

if ARM
# arm needs to build some stuff even in JIT mode
SUBDIRS = arm
endif

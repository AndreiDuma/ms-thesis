SForth: SForth.dmp system.fs
	./build.sh

run: SForth
	./SForth

gdb: SForth
	gdb -x .gdbinit ./SForth

dbg: SForth
	./SForth | xxd -u -g8 -o 0x0fffffe0 -R always

reg: SForth
	./SForth | xxd -u -g8 -e

clean:
	rm SForth SForth0

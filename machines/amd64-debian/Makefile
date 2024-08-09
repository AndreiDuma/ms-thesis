amd64-debian:
	qemu-system-x86_64 \
		-machine 'pc' \
		-cpu 'Nehalem' \
		-m 1G \
		-drive file=dqib_amd64-pc/image.qcow2 \
		-device e1000,netdev=net \
		-netdev user,id=net,hostfwd=tcp::2222-:22 \
		-kernel dqib_amd64-pc/kernel \
		-initrd dqib_amd64-pc/initrd \
		-nographic \
		-append "root=LABEL=rootfs console=ttyS0"

SHELL            := cmd.exe
export SHELL

.phony: all

all:
	@$(MAKE) -fmakefile_T40.mk --no-print-directory
	@$(MAKE) -fmakefile_T41.mk --no-print-directory
	@$(MAKE) -fmakefile_TMM.mk --no-print-directory

clean:
	@$(MAKE) -fmakefile_T40.mk --no-print-directory clean
	@$(MAKE) -fmakefile_T41.mk --no-print-directory clean
	@$(MAKE) -fmakefile_TMM.mk --no-print-directory clean
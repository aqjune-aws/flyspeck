# Build of the nonlinear inequality verifiers against the HOL Light in
# HOLLIGHT_DIR, using that HOL Light's own native compilation support:
# "hol.sh inline-load" folds the needs/loadt statements of a driver into one
# file, "hol.sh compile" turns it into a .cmx against hol_lib.cmxa, and
# ocamlfind links it.
#
#   make HOLLIGHT_DIR=/path/to/hol-light
#
# "all" builds native_verifier, for the 23237 cases of ineqs.txt.  The five
# sharp inequalities need sharp_verifier, which "all" leaves alone:
#
#   make sharp_verifier
#
# Str is needed by flyspeck-nat/serialization.hl and flyspeck-nat/ineq_data.hl,
# which is why the link line names more packages than "hol.sh link" does.

HOLLIGHT_DIR ?= /mnt/disk/hol-light
HOL_SH = $(HOLLIGHT_DIR)/hol.sh

SRC = native_verifier.hl
RAW = native_verifier_raw.ml
GEN = native_verifier_inlined.ml
OBJ = native_verifier_inlined.cmx
BIN = native_verifier

SHARP_SRC = sharp_verifier.hl
SHARP_RAW = sharp_verifier_raw.ml
SHARP_GEN = sharp_verifier_inlined.ml
SHARP_OBJ = sharp_verifier_inlined.cmx
SHARP_BIN = sharp_verifier

DEPS = flyspeck-nat/prove_by_refinement.hl \
       flyspeck-nat/definitions.hl \
       flyspeck-nat/serialization.hl \
       flyspeck-nat/break_case.hl \
       flyspeck-nat/hminus.hl \
       flyspeck-nat/ineq_data.hl

SHARP_DEPS = flyspeck-nat/prove_by_refinement.hl \
	     flyspeck-nat/definitions.hl \
	     flyspeck-nat/serialization.hl \
	     flyspeck-nat/break_case.hl \
	     flyspeck-nat/hminus.hl \
	     flyspeck-nat/sharp_theorems.hl \
	     flyspeck-nat/sharp_ineqs.hl

all: $(BIN)

$(RAW): $(SRC) $(DEPS)
	$(HOL_SH) inline-load $(SRC) $(RAW)

$(SHARP_RAW): $(SHARP_SRC) $(SHARP_DEPS)
	$(HOL_SH) inline-load $(SHARP_SRC) $(SHARP_RAW)

# Toplevel directives have no meaning in compiled code: unix comes from the link
# line and the printers are for interactive use.  Blanking the lines rather than
# deleting them keeps the line numbers that inline-load emits accurate.
$(GEN): $(RAW)
	sed -E 's/^[[:space:]]*#(load|install_printer|directory|use)[[:space:]].*$$//' \
		$(RAW) > $(GEN)

$(SHARP_GEN): $(SHARP_RAW)
	sed -E 's/^[[:space:]]*#(load|install_printer|directory|use)[[:space:]].*$$//' \
		$(SHARP_RAW) > $(SHARP_GEN)

$(OBJ): $(GEN)
	$(HOL_SH) compile $(GEN)

$(SHARP_OBJ): $(SHARP_GEN)
	$(HOL_SH) compile $(SHARP_GEN)

$(BIN): $(OBJ)
	ocamlfind ocamlopt -package zarith,unix,str -linkpkg \
		-I $(HOLLIGHT_DIR) $(HOLLIGHT_DIR)/hol_lib.cmxa $(OBJ) -o $(BIN)

$(SHARP_BIN): $(SHARP_OBJ)
	ocamlfind ocamlopt -package zarith,unix,str -linkpkg \
		-I $(HOLLIGHT_DIR) $(HOLLIGHT_DIR)/hol_lib.cmxa $(SHARP_OBJ) -o $(SHARP_BIN)

clean:
	rm -f $(RAW) $(GEN) $(BIN) native_verifier_inlined.cmi \
		native_verifier_inlined.cmx native_verifier_inlined.o \
		$(SHARP_RAW) $(SHARP_GEN) $(SHARP_BIN) sharp_verifier_inlined.cmi \
		sharp_verifier_inlined.cmx sharp_verifier_inlined.o

.PHONY: all clean

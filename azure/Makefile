# Build of the nonlinear inequality verifier against the HOL Light in
# HOLLIGHT_DIR, using that HOL Light's own native compilation support:
# "hol.sh inline-load" folds the needs/loadt statements of native_verifier.ml
# into one file, "hol.sh compile" turns it into a .cmx against hol_lib.cmxa, and
# ocamlfind links it.
#
#   make HOLLIGHT_DIR=/path/to/hol-light
#
# Str is needed by flyspeck-nat/serialization.hl and flyspeck-nat/ineq_data.hl,
# which is why the link line names more packages than "hol.sh link" does.

HOLLIGHT_DIR ?= /mnt/disk/hol-light
HOL_SH = $(HOLLIGHT_DIR)/hol.sh

SRC = native_verifier.ml
RAW = native_verifier_raw.ml
GEN = native_verifier_inlined.ml
OBJ = native_verifier_inlined.cmx
BIN = native_verifier

DEPS = compat.ml \
       flyspeck-nat/prove_by_refinement.hl \
       flyspeck-nat/definitions.hl \
       flyspeck-nat/serialization.hl \
       flyspeck-nat/break_case.hl \
       flyspeck-nat/hminus.hl \
       flyspeck-nat/ineq_data.hl

all: $(BIN)

$(RAW): $(SRC) $(DEPS)
	$(HOL_SH) inline-load $(SRC) $(RAW)

# Toplevel directives have no meaning in compiled code: unix comes from the link
# line and the printers are for interactive use.  Blanking the lines rather than
# deleting them keeps the line numbers that inline-load emits accurate.
$(GEN): $(RAW)
	sed -E 's/^[[:space:]]*#(load|install_printer|directory|use)[[:space:]].*$$//' \
		$(RAW) > $(GEN)

$(OBJ): $(GEN)
	$(HOL_SH) compile $(GEN)

$(BIN): $(OBJ)
	ocamlfind ocamlopt -package zarith,unix,str -linkpkg \
		-I $(HOLLIGHT_DIR) $(HOLLIGHT_DIR)/hol_lib.cmxa $(OBJ) -o $(BIN)

clean:
	rm -f $(RAW) $(GEN) $(BIN) native_verifier_inlined.cmi \
		native_verifier_inlined.cmx native_verifier_inlined.o

.PHONY: all clean

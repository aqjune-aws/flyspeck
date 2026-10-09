# Step by Step Installation Guide

This guide was tested with OCaml 5.04 on Ubuntu 24.04.

Install [opam](https://ocaml.org/docs/installing-ocaml#1-install-opam).

## HOL Light

1) Clone the HOL Light repository

    ```
    git clone https://github.com/jrh13/hol-light.git
    ```

   The proof was tested against commit
   `cba9198db76e9dfb89cbd653df9412d01f65b22a`, with camlp5 8.04.00 and
   zarith 1.14.  `azure/config.ml` records the same commit, since the
   nonlinear inequalities have to be imported into the HOL Light that
   verified them.

2) Initialize HOL Light

    ```
    cd hol-light
    make switch-5
    HOLLIGHT_USE_MODULE=1 make
    ```

## Loading Flyspeck

1) Point `FLYSPECK_DIR` at the Flyspeck `text_formalization` directory and
   `HOLLIGHT_DIR` at the HOL Light one.  `load_flyspeck.ml` reads both from the
   environment and stops if either is unset or is not a directory.

    ```
    export FLYSPECK_DIR={path to flyspeck}/text_formalization
    export HOLLIGHT_DIR={path to HOL Light}
    ```

2) Run `{path to HOL Light}/hol.sh` from the Flyspeck directory.

    ```
    cd flyspeck
    {path to HOL Light}/hol.sh
    ```

3) Wait until HOL Light is loaded and then initialize Flyspeck

    ```
    needs "load_flyspeck.ml";;
    ```

    This command will take a relatively long time since the full 
    HOL Light multivariate analysis library is loaded.

4) To load the main statement, use the following command:

    ```
    build_to "general/the_main_statement.hl";;
    ```

    To load the main statement with linear program bounds, use the command:

    ```
    build_to_full "general/the_kepler_conjecture.hl";;
    ```

    For the whole proof, load the sequence itself:

    ```
    map flyspeck_needs Build.build_sequence_full;;
    ```

   The comments in `load_flyspeck.ml` say which theorem each of the three ends at.

## [Optional] Regenerating the computations

Two parts of the proof are computations whose results are kept in the
repository, so that loading the project needs neither of the tools that
produced them.  Both can be redone.

[`formal_lp`](formal_lp) is the linear programming.  A certificate for each of
the 19715 linear programs sits in
[`formal_lp/glpk/binary`](formal_lp/glpk/binary), and
`tame/linear_programming_results.hl` reads them all and proves
`linear_programming_results` as a step of the build.  Building the certificates
needs GLPK and a C# program run under mono.
[`formal_lp/README.txt`](formal_lp/README.txt) has the instructions,
and [`formal_lp/glpk/README.md`](formal_lp/glpk/README.md) covers the models.

[`azure`](azure) is the nonlinear inequalities, first verified on the Microsoft
Azure cloud at a cost of some 5000 processor-hours. Rather than reproving
them, `nonlinear/mk_all_ineq.hl` imports them against the md5 digests in
`general/theorem_nonlinear_digest.hl`.
This installs the `deserialization` axiom that `warn_axiom` reports. 
Verifying them again and collecting the digests, is described in
[`azure/README.md`](azure/README.md).

## [Optional] Checkpointing with DMTCP

You can use [DMTCP](https://github.com/dmtcp/dmtcp/blob/master/INSTALL.md) and
[checkpoint_flyspeck](text_formalization/build/checkpoint_flyspeck) to 
checkpoint the OCaml REPL session.

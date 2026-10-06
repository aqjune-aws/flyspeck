Verification of the Flyspeck nonlinear inequalities
===================================================

`native_verifier` proves the Flyspeck nonlinear inequalities with HOL Light's
`Formal_ineqs` and prints the md5 digest of each theorem it obtains. Those
digests are what `text_formalization/general/serialization.hl` checks when
`nonlinear/mk_all_ineq.hl` imports the inequalities, so a complete run of this
directory is what lets that file discharge `the_nonlinear_inequalities`.

## Build

Requirements:

- a HOL Light tree built with `HOLLIGHT_USE_MODULE=1`, which is what produces
  the `hol_lib.cmxa` this links against:

      make -C /path/to/hol-light HOLLIGHT_USE_MODULE=1

- GNU parallel, for the parallel verification below.

- the OCaml toolchain HOL Light was built with, on the PATH, with `ocamlfind`
  and the `zarith`, `unix` and `str` packages. For a HOL Light with its own opam
  switch:

      eval $(opam env --switch /path/to/hol-light --set-switch)

Then:

    make HOLLIGHT_DIR=/path/to/hol-light

The build folds the loads of `native_verifier.hl` into a single compilation unit
with `hol.sh inline-load`, compiles that against `hol_lib.cmxa` with
`hol.sh compile`, and links it with `ocamlfind`. The result, `./native_verifier`,
needs only a system `libgmp` at run time.

## Verification of individual inequalities

Which cases to verify, and which file to read them from, come from the
environment variables:

| variable         | meaning                          | default      |
| ---------------- | -------------------------------- | ------------ |
| `FLYSPECK_INEQS` | data file of inequalities        | `ineqs.txt`  |
| `FLYSPECK_FIRST` | first case, counting from 0      | `0`          |
| `FLYSPECK_LAST`  | last case                        | `0`          |

For example:

    FLYSPECK_INEQS=results/ineqs/ineqs2_trig.txt FLYSPECK_FIRST=0 \
      FLYSPECK_LAST=13 ./native_verifier > out.txt

Case 0 of `ineqs.txt` takes 150 s on a Neoverse-V2 core, and a process spends
about 340 s building the base 200 arithmetic tables before it starts on any
case. Peak resident size is around 1.2 GB.

## Parallel verification of all strict inequalities

    ./run-parallel 64

The argument is the number of concurrent jobs.
`run-parallel` hands GNU parallel the 609 case
ranges of `pars.txt`, which between them cover `ineqs.txt`, and collects the
output under `out/`, one directory per range.

Choose the number of jobs for memory rather than for cores: a process usually
needs about 1.2 GB, and a range of hard cases needs more, which is why
`run-parallel` passes `--memfree` as well.

Any further argument goes to GNU parallel.  Ranges that finish are recorded in
`run-parallel.joblog`.

## Collecting the digests

From the `out/` dir, all hashes must be collected and stored at `hashes.txt`:

    find out -type f ! -regex ".*/\..*" -exec grep "Hash" '{}' \; \
      | sort -V | sed -e 's/.*Hash  //' -e 's/^.*,(/(/' > hashes.txt

Each line of the result pairs a case with the md5 digest of its theorem.

## The sharp inequalities

Five of the nonlinear inequalities are sharp: they hold with equality somewhere
on their domain, so the interval arithmetic that settles the other 23237 cannot
settle them.  They are not in `ineqs.txt`, which is why `results/hashes.txt`
holds 23242 entries against that file's 23237 cases:

    (prep-GRKIBMP B V2,0)
    (prep-OMKYNLT 3336871894,0)
    (prep-QZECFIC wt0 corner,0)
    (prep-TSKAJXY-IYOUOBF sharp v2,0)
    (prep-TSKAJXY-RIBCYXU sharp,0)

`theorem_nonlinear_digest.hl` has to cover these five as well, so a `hashes.txt`
built from `out/` alone is five lines short, and a digest list regenerated from
it would leave `mk_all_ineq.hl` unable to import them.

`sharp_verifier.hl` covers them: it loads the prelude of `native_verifier.hl`,
then `flyspeck-nat/sharp_theorems.hl`, which proves the exact equality each of
the five needs, and `flyspeck-nat/sharp_ineqs.hl`, which verifies the cases and
prints `Hash <name>: <md5>` as it loads.  All five run in one process, in about
7 minutes:

    make sharp_verifier
    ./sharp_verifier > out_sharp.txt

Append its digests to the `hashes.txt` collected above, bringing that file to
23242 lines:

    grep "Hash" out_sharp.txt | sed -e 's/^.*,(/(/' >> hashes.txt

## Comparing with the digests held in the project

    cat hashes.txt | sed -e 's/^.*: *//' | sort > hash1.txt
    cat ../text_formalization/general/theorem_nonlinear_digest.hl \
      | grep -F '"' | sed -e 's/^.*:", *"//' -e 's/".*$//' | sort > hash2.txt
    md5sum hash1.txt hash2.txt

`results/hashes.txt`, from the 2014 run, agrees with
`theorem_nonlinear_digest.hl` this way. A fresh run does not reproduce either of
them, because a digest covers the entire definitional history of its theorem and
HOL Light commit a84e0f3 replaced `define_finite_type` with `tybit0`/`tybit1`,
changing the history of everything that mentions `real^N`. Regenerating
`theorem_nonlinear_digest.hl` from a fresh `hashes.txt` is the point of running
this directory.

## Files that nothing builds

- `main_verifier.hl` and `config.ml`, which `native_verifier.hl` supersedes.
- `test_flyspeck.hl`, a performance test.

# formal_lp/glpk

The informal half of the linear programming work: it solves the linear programs
and writes a certificate for each one.  Nothing here is part of the proof - the
formal half, which reads these certificates, is `../hypermap`.

`build_main.hl` is the entry point, and `../README.txt` says how to run it.  It
needs GLPK on the PATH and `../LP-HL/LP-HL/bin/Release/LP-HL.exe`, which it runs
under mono.  The certificates it writes are kept in `binary/`, so verifying the
linear programs needs neither tool.

`binary/` is some four hundred megabytes.  One certificate there is over the
hundred megabytes GitHub takes, so it is kept as a `.tar.gz`, which
`read_lp_certificates` unpacks itself; only one of the two forms may be
present, since `Verify_all` reads every `easy` and `hard` file it finds.

## The two models

`make_models false`, which `build_all` calls, writes both of the models a run
reads, and the `false` matters: it strips the `main:` constraint, `sum ln >=
12`, from `graph_all.mod` as well as from `model2.mod`.  A branch solve against
a `graph_all.mod` that still has `main:` reports no primal feasible solution
for every branch whose maximum falls just short of 12 - which is to say for
every branch the search resolves - instead of reporting the maximum.  Those
branches are then certified by the route meant for genuinely inconsistent
constraints, and since the slack problem is built from `model2.mod`, which has
no `main:` either, it is feasible, its optimum is zero, and the certificate
proves nothing.  `verify_lp_certificate` rejects the result.

## Running in parallel

One run is serial, and the hard cases are long: the longest needs more than
twelve thousand branch solves.  `build_and_save_all` takes a list of cases, so
the cases can be split across processes, as long as each process has its own

- `tmp_dir`, directly under this directory so that the `tmp_dir/../sed_*.sed`
  of `build_terminal_case` still resolves, holding a copy of `tmp/000.txt`,
  which LP-HL.exe requires;
- `model2_path`, its own copy of `model2.mod`;
- `output_dir`, because file numbering starts from 1 in every run.

`graph_all.mod` is shared, and `make_models` rewrites it, so run `make_models
false` once before starting any of the processes and let none of them call it.
The `glpk_outfile` of `lpproc.ml` is a per process temporary and needs nothing
done.

Merge the directories afterwards with

    ./merge_certificates.sh binary out_dir_1 out_dir_2 ...

which renumbers the files so that the result looks like one run.  It only moves
files; that every merged file still reads back is worth checking separately,
with `Lp_certificate.read_lp_certificates`.

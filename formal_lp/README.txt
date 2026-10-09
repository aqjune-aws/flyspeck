Formal verification of Flyspeck linear programs.

It is assumed that a global variable "flyspeck_dir" contains
a path to the "text_formalization" directory of the Flyspeck
project.

-----------------------------------------------------
0. What is in each directory
-----------------------------------------------------

The work is in two halves.  glpk/ and LP-HL/ solve the linear programs and
write a certificate for each one; nothing there is part of the proof.
hypermap/, ineqs/ and more_arith/ are the formal half, which reads those
certificates and proves the corresponding theorem in HOL Light.

glpk/
  The GLPK side.  head.mod, body.mod and tail.mod are the model, from which
  make_models assembles graph_all.mod and model2.mod; those two are generated
  and not kept in the repository.  glpk_link.ml runs glpsol and reads back what
  it says, lpproc.ml and hard_lp.ml drive the branch and bound over a
  hypermap, and sphere.ml holds the constants they need.  build_certificates.hl
  turns one branch into a certificate and build_main.hl is the entry point,
  Lp_build_main.build_all.  The three .sed scripts rewrite a model into the
  slack form used when a branch is infeasible.  lp_binary_certificate.hl reads
  what LP-HL.exe writes.  binary/ holds the certificates themselves, tmp/ is
  the working directory of a run and keeps 000.txt, which LP-HL.exe requires,
  and ex1/ and ex2/ are small examples.

LP-HL/
  A C# program, LP-HL.exe, which reads a model and the solution glpsol wrote
  for it and emits the binary certificate that binary/ is made of.  LP-HL.sln
  and LP-HL/*.cs are its sources, and LP-HL/bin/Release/LP-HL.exe the build
  that build_certificates.hl runs under mono.

hypermap/
  The formal side.  verify_all.hl is the entry point, Verify_all.verify_all.
  main/ reads a certificate (lp_certificate.hl) and proves the theorem it
  stands for (prove_flyspeck_lp.hl), with its tests.  ineqs/ defines the
  lp_ineqs and lp_main_estimate constants and proves what is claimed of them.
  computations/ evaluates the list and hypermap functions the proofs apply to
  explicit hypermaps.  ssreflect/ holds the hypermap theory, each
  *-compiled.hl generated from the *.vhl beside it.  arith_link.hl sets the
  arithmetic up from the Formal_ineqs of the HOL Light being built against.

ineqs/
  Interval arithmetic for the constants the linear programs quote
  (constants_approx.hl) and the delta inequality (delta_ineq.hl).

more_arith/
  The arithmetic the formal half needs over and above HOL Light's: integers
  (arith_int.hl), linear forms (lin_f.hl), and proving a linear program from
  its certificate (prove_lp.hl).

lp_example/
  One linear program carried through the whole process by hand, useful for
  seeing what the files above pass to each other.

-----------------------------------------------------
I. Construction of linear program certificates
-----------------------------------------------------

This step can be skipped. All linear program certificates
are in the repository at 
flyspeck_dir/../formal_lp/glpk/binary

All certificate files are binary files which contain serialized
OCaml data structures, and they hold zarith integers, which are
custom blocks.

All certificates can be reconstructed manually in the following way.

0) Install glpk (http://www.gnu.org/software/glpk/) and
mono (http://www.mono-project.com/Main_Page).

1) Make sure that the environment has the FLYSPECK_DIR variable.
This variable must contain a path to the "text_formalization" directory
of the Flyspeck project.

2) Start HOL Light with its hol.sh.

3) Load the file build_main.hl:
needs (Filename.concat (Sys.getenv "FLYSPECK_DIR")
         "../formal_lp/glpk/build_main.hl");;

4) Build all linear program certificates with the command
Lp_build_main.build_all 1000;;

Here, 1000 is a parameter which specifies how many terminal cases will be
saved in each certificate file for easy linear program certificates.
All certificates will be saved in 
flyspeck_dir/../formal_lp/glpk/binary
Certificates of easy linear programs will be saved in files with the prefix "easy".
Certificates of hard linear programs will be saved in files with the prefix "hard".

A rebuild writes .dat files, and one certificate is kept compressed because of
its size, so delete that .tar.gz once its .dat is in place: Verify_all reads
every file in binary/ whose name begins with "easy" or "hard", and would
otherwise read the certificate in both forms.

It is also possible to build either easy linear program certificates or hard linear
program certificates. The corresponding commands are
Lp_build_main.build_all_easy 1000;;
Lp_build_main.build_all_hard 1;;

The run is serial and the hard cases take hours. glpk/README.md says what a
process needs of its own to run several of them at once, and comes with a
script that merges the output directories afterwards.

-----------------------------------------------------
II. Formal verification of Flyspeck linear programs
-----------------------------------------------------

0) Make sure that the directory flyspeck_dir/../formal_lp/glpk/binary
contains all certificate files.

1) Start OCaml and load the Flyspeck project; ../load_flyspeck.ml is the entry
point for that.

2) needs "../formal_lp/hypermap/verify_all.hl";;

3) let result = Verify_all.verify_all [] None;;
Verifying all of them took three hours on one core of an EC2 r7i instance.
tame/linear_programming_results.hl does the same verification as a step of
the project build, and assembles the result into the theorem
`linear_programming_results`.

Commands
let result_easy = Verify_all.verify_easy [] None;;
let result_hard = Verify_all.verify_hard [] None;;
will verify easy and hard linear programs separately.

result has the type ((string * thm) list * float) list. Each element of the list
contains a list of theorems for all certificates in one data file and
the verification time for this data file.

All theorems should be in the form
lp_ineqs, lp_main_estimate, 
  iso (hypermap_of_fan (V,ESTD V)) (hypermap_of_list L) |- contravening V ==> F
Here, L is an explicit list of lists of numbers which encodes a particular hypermap.
`lp_ineqs` is a constant which defines all nonlinear inequality required for
verification of linear programs.
`lp_main_estimate` is a constant which defines the main estimate conclusions.

The command
Verify_all.test_result result;;
will return the total verification time, 
a union of all conclusions (must be `contravening V ==> F`),
and a union of all assumptions without isomorphism assumptions 
(must be [`lp_ineqs`; `lp_main_estimate`]).

(* ========================================================================= *)
(* Glue that lets azure/flyspeck-nat load into one flat HOL Light scope.      *)
(*                                                                          *)
(* The flyspeck-nat sources expect two things of their surroundings:          *)
(*                                                                          *)
(*  1. Modules named Hol_core, Vectors and Transcendentals, holding parts of   *)
(*     HOL Light that it keeps at the top level.  The modules below carry the  *)
(*     names that the sources reach for through those three, each bound to the *)
(*     global of the same name.                                               *)
(*                                                                          *)
(*  2. A module per source file, which is what OCaml gives a separately       *)
(*     compiled unit.  The files are loaded into one scope here, so the names *)
(*     that are used qualified are re-exported by hand; see the small modules *)
(*     that follow each load in the driver.                                   *)
(* ========================================================================= *)

module Hol_core = struct
  let parse_term = parse_term
end;;

module Vectors = struct
  let POW_2_SQRT_ABS = POW_2_SQRT_ABS
  let SQRT_LE_0 = SQRT_LE_0
  let SQRT_MONO_LT_EQ = SQRT_MONO_LT_EQ
  let SQRT_MUL = SQRT_MUL
  let SQRT_POW_2 = SQRT_POW_2
end;;

module Transcendentals = struct
  let ACS_ATN = ACS_ATN
  let ATN_NEG = ATN_NEG
end;;

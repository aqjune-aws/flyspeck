#!/bin/bash

# Merge the certificate directories of several parallel runs into one.
#
#     merge_certificates.sh <output dir> <input dir>...
#
# build_and_save_all numbers its files <prefix>_1.dat, <prefix>_2.dat and so on
# from 1 in every run, so directories from runs in parallel cannot just be
# copied together.  Files are grouped by prefix and renumbered from 1, taking
# the input directories in the order given.
#
# Whether the result reads back is a separate question; see README.md.

set -e

if [ $# -lt 2 ]; then
    echo "usage: $0 <output dir> <input dir>..." >&2
    exit 1
fi

out="$1"
shift
mkdir -p "$out"

prefixes=$(for d in "$@"; do
               ls "$d" 2>/dev/null || true
           done | sed -n 's/^\(.*\)_[0-9][0-9]*\.dat$/\1/p' | sort -u)

if [ -z "$prefixes" ]; then
    echo "no <prefix>_<n>.dat files in: $*" >&2
    exit 1
fi

for prefix in $prefixes; do
    n=0
    for d in "$@"; do
        files=$(ls "$d/${prefix}"_*.dat 2>/dev/null || true)
        for f in $(echo "$files" | sed 's/.*_\([0-9][0-9]*\)\.dat$/\1 &/' \
                   | sort -n | cut -d' ' -f2-); do
            n=$((n + 1))
            cp "$f" "$out/${prefix}_${n}.dat"
        done
    done
    echo "$prefix: $n file(s) in $out"
done

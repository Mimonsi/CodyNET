# !/usr/bin/env bash
set -euo pipefail

# Self-healing: Remove BOM if present
if [ -f "$0" ]; then
    sed -i '1s/^\xEF\xBB\xBF//' "$0" 2>/dev/null || true
fi

# Check if unzip is available, install if missing
if ! command -v unzip &> /dev/null; then
    echo "unzip is required to automatically download and unpack the test data. Please confirm the installation."
    echo "unzip not found. Installing..."
    sudo apt update && sudo apt install -y unzip
fi

ROOT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
TARGET_DIR="${ROOT_DIR}/wdc65c02/v1"

if [ -d "${TARGET_DIR}" ] && ls "${TARGET_DIR}"/*.json >/dev/null 2>&1; then
    echo "Single step test data already present at ${TARGET_DIR}."
    exit 0
fi

TMP_DIR=$(mktemp -d)
cleanup() {
    rm -rf "${TMP_DIR}"
}
trap cleanup EXIT

ARCHIVE="${TMP_DIR}/65x02.zip"
SOURCE_URL="https://github.com/SingleStepTests/65x02/archive/refs/heads/main.zip"

echo "Downloading single step test data... This may take a few minutes"
curl -L -o "${ARCHIVE}" "${SOURCE_URL}"
unzip -q "${ARCHIVE}" -d "${TMP_DIR}"

SOURCE_DIR=$(find "${TMP_DIR}" -type d -path "*/wdc65c02/v1" | head -n 1)

if [ -z "${SOURCE_DIR}" ]; then
    echo "Could not locate wdc65c02/v1 in downloaded archive."
    exit 1
fi

mkdir -p "${TARGET_DIR}"
cp -a "${SOURCE_DIR}/." "${TARGET_DIR}/"

echo "Downloaded single step test data to ${TARGET_DIR}."
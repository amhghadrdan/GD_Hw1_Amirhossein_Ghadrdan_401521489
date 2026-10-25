#!/usr/bin/env python3
"""Compatibility entry point for generating the Persian PDF report."""
from pathlib import Path
import runpy
runpy.run_path(str(Path(__file__).with_name('create_pdf_report.py')), run_name='__main__')

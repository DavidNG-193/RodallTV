from __future__ import annotations

import logging
from pathlib import Path


def configure_logging(runtime_dir: Path) -> None:
    log_dir = runtime_dir.parent / "logs"
    log_dir.mkdir(parents=True, exist_ok=True)
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s %(levelname)s %(name)s: %(message)s",
        handlers=[
            logging.StreamHandler(),
            logging.FileHandler(log_dir / "rodall-agent.log", encoding="utf-8"),
        ],
    )
# Permisos de energía

Instala la política restringida en cada Raspberry Pi:

```bash
sudo install -o root -g root -m 0440 \
  deploy/rodall-power.sudoers \
  /etc/sudoers.d/rodall-power

sudo visudo -cf /etc/sudoers.d/rodall-power
```

La validación debe finalizar con:

```text
/etc/sudoers.d/rodall-power: parsed OK
```

La política permite al usuario `rodall` ejecutar únicamente el reinicio y
apagado mediante `/usr/bin/systemctl`. No concede acceso general a `sudo`.

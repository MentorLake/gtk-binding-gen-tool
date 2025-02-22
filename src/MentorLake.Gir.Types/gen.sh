#!/bin/bash

trang ./gir.rnc core.xsd

xscgen -n "|core.xsd=MentorLake.Gir.Core" \
	-n "|c.xsd=MentorLake.Gir.C" \
	-n "|glib.xsd=MentorLake.Gir.GLib" \
	-n "|xml.xsd=MentorLake.Gir.Xml" \
	--o GirTypes \
	--cn \
	--uc \
	-ct "System.Collections.Generic.List\`1" \
	--sf \
	--nh \
	./core.xsd

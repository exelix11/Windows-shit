function UncheckAll() {	
	var inputs = document.getElementsByTagName('input');
	for (var i = 0; i < inputs.length; i++) {
		if (inputs[i].type == 'checkbox') {
			inputs[i].checked = false;
		}
	}
}

function Init() {
	UncheckAll();
	document.getElementById("preset").value = "NONE";
}

const Presets = {
	$JS_PRESETS
};

const IdToCommand = $JS_COMMANDS
;

const IdToRegUser = $JS_REG_USER
;

const IdToRegAdmin = $JS_REG_ADMIN
;

function ApplyPreset(preset) {
	UncheckAll();
	for (var i = 0; i < Presets[preset].length; i++) {
		document.getElementById(Presets[preset][i]).checked = true;
	}
}

function OnPresetChanged() {
	var preset = document.getElementById("preset").value;
	ApplyPreset(preset);
}

function DownloadString(name, content)
{
	var element = document.createElement('a');
	element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(content));
	element.setAttribute('download', name);
	element.style.display = 'none';
	document.body.appendChild(element);
	element.click();
	document.body.removeChild(element);	
}

function GenerateReg(kind)
{
	res = "Windows Registry Editor Version 5.00\r\n";
	res += "; Script from https://exelix11.github.io/Windows-shit/ \r\n";

	admin = kind == "admin" || kind == "all";
	user = kind == "user" || kind == "all";

	if (user)
	{
		for (var id in IdToRegUser) {
			if (document.getElementById(id).checked) {
				res += IdToRegUser[id] + "\r\n";
			}
		}
	}

	if (admin)
	{
		for (var id in IdToRegAdmin) {
			if (document.getElementById(id).checked) {
				res += IdToRegAdmin[id] + "\r\n";
			}
		}
	}

	DownloadString("reg_" + kind + ".reg", res);
}

function GenerateBat() 
{
	res = "@echo off\r\n";
	res += "echo Script from https://exelix11.github.io/Windows-shit/ \r\n";
	
	// IdToCommand is a map of ID to the text to add to res if checked
	for (var id in IdToCommand) {
		if (document.getElementById(id).checked) {
			res += IdToCommand[id] + "\r\n";
		}
	}

	res += "echo FINISHED\r\npause\r\n";

	DownloadString("commands.bat", res);
}
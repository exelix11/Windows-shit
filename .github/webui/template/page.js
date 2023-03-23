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

const IdToString = $JS_COMMANDS
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

function Generate() 
{
	res = "@echo off\r\n";
	res += "echo Script from https://exelix11.github.io/Windows-shit/ \r\n";
	
	// IdToString is a map of ID to the text to add to res if checked
	for (var id in IdToString) {
		if (document.getElementById(id).checked) {
			res += IdToString[id] + "\r\n";
		}
	}

	res += "echo FINISHED\r\npause\r\n";

	// Download res as a text file
	var element = document.createElement('a');
	element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(res));
	element.setAttribute('download', "commands.bat");
	element.style.display = 'none';
	document.body.appendChild(element);
	element.click();
	document.body.removeChild(element);
}
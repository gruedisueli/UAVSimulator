//used examples found at:
//https://pixeleuphoria.com/blog/index.php/2020/04/27/unity-webgl-download-content/
//and
//https://pixeleuphoria.com/blog/index.php/2020/04/29/unity-webgl-upload-content/

mergeInto(LibraryManager.library, 
{
    BrowserTextDownload: function(filename, textContent)
    {
        // https://ourcodeworld.com/articles/read/189/how-to-create-a-file-and-generate-a-download-with-javascript-in-the-browser-without-a-server
        
        // Convert paramters to the correct form. See Unity WebGL Plugins page
        // for more information. It's not too important to realize why you need 
        // to do this, as long as you know THAT you need to.
        var strFilename = Pointer_stringify(filename);
        var strContent = Pointer_stringify(textContent);

        // Create the hyperlink for a user to click
        var element = document.createElement('a');
        
        // Set the link destination as hard-coded file data.
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(strContent));
        element.setAttribute('download', strFilename);
        
        // Make sure it's not visible when added to the HTML body
        element.style.display = 'none'; 
        
        // Activate it by adding it to the HTML body
        document.body.appendChild(element);
        // Don't wait for the user to click it, activate it ourselves!
        element.click();
        // Clean up our mess, now that the anchor's purpose is finished.
        document.body.removeChild(element);
    },
BrowserTextUpload: function(extFilter, gameObjName, dataSinkFn)
    {
        // If this is the first time being called, create the reusable 
        // input DOM object.
        if(typeof inputLoader == "undefined")
        {
            inputLoader = document.createElement("input");
            // We need it to be of the "file" type to have to popup dialog
            // and file uploading features we need.
            inputLoader.setAttribute("type", "file");
            // When we add it to the body, make sure it doesn't visually
            // affect the page.
            inputLoader.style.display = 'none';
            // We need to add it to the body in order for it to be active.
            document.body.appendChild(inputLoader);

            // Setup the callback
            inputLoader.onchange = 
                function(x)
                {
                    // If empty, nothing was selected.
                    if(this.value == "")
                        return;

                    // In this example, we assume only one file is selected
                    var file = this.files[0];
                    // The file data isn't instantly available at this level.
                    // In order to access the file contents, we need to run
                    // it through the FileReader and process it in its onload
                    // callback (this is callback-ception)
                    var reader = new FileReader();
                    // Clear the value to empty. Because onchange only triggers if
                    // the selection is different. So if the same file is selected again
                    // afterwards for legitimate reasons, it wouldn't trigger an onchange
                    // if we don't clear this.
                    this.value = "";
                    // Giving reader.onload access to this input.
                    var thisInput = this;
                    
                    reader.onload = function(evt) 
                    {
	                    if (evt.target.readyState != 2)
		                    return;

	                    if (evt.target.error) 
	                    {
		                    alert("Error while reading file " + file.name + ": " + loadEvent.target.error);
		                    return;
	                    }
	                    
	                    // The text results are in evt.target.result, so just send that
	                    // back into our app with SendMessage(). Note that we DON'T need
	                    // to call Pointer_stringify() on it.
                        unityInstance.SendMessage(
                            inputLoader.gameObjName, 
                            inputLoader.dataSinkFn, 
                            evt.target.result);
                    }
                    reader.readAsText(file);
                }
        }
        // We need to turn these values into strings before the callback, because the
        // memory that these parameter-string-pointers point to will not be valid later.
        inputLoader.gameObjName = Pointer_stringify(gameObjName);
        inputLoader.dataSinkFn = Pointer_stringify(dataSinkFn);
        // Set the extension filtering for the upload dialog
        inputLoader.setAttribute("accept", Pointer_stringify(extFilter))
        // Force the input object to activate and open the upload dialog.
        inputLoader.click();
    },
});
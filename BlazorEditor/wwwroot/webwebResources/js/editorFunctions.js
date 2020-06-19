let editorModels = [];
let editor;
let currentTabID = 0;
const subtabs = ["Razor", "CSS", "JS"];

class editorModel {
    constructor(contents, lang) {
        this.model = monaco.editor.createModel(contents, lang);
        this.viewState = undefined;
    }
}

class editorTab {
    constructor(title, contents, isLayout) {
        this.title = title;
        this.models = [];
        subtabs.forEach((st, n) => {
            this.models.push(new editorModel(contents[n], st.toLowerCase()));
        });
        this.isLayout = isLayout
        this.subtab = 0;
    }
}

async function closeAllModalsAtOnce() {
    $('#manageFilesModal').modal('hide modal', () => {}, false, true);
    $('#configureModal').modal('hide modal', () => {}, false, true);
    $('#manageAccountModal').modal('hide modal', () => {}, false, true);
    $('#manageItemsModal').modal('hide modal', () => {}, false, true);
    $('#startupModal').modal('hide modal', () => {}, false, true);
    $('#openModal').modal('hide modal', () => {}, false, true);
    $('#newModal').modal('hide modal', () => {}, false, true);
    $('body').dimmer('get dimmer').dimmer('hide');
}

let openModalWasApproved = false;
let newModalWasApproved = false;

function monacoInit() {
    $('.ui.dropdown').dropdown();
    $('#mainMenu').dropdown({
        action: 'hide'
    });
    $('.ui.checkbox').checkbox();
    $('#configureModalTabs .item').tab();
    $('#openModal').modal({
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetOpenModalState');
            if(openModalWasApproved) {
                $('#startupModal').modal('hide');
            }
        },
        onApprove: () => {
            openModalWasApproved = true;
            if(editorModels.length === 0) {
                closeAllModalsAtOnce();
                return false;
            }
        },
        allowMultiple: true
    });
    $('#newModal').modal({
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetNewModalState');
            if(newModalWasApproved) {
                $('#startupModal').modal('hide');
            }
        },
        onApprove: () => {
            newModalWasApproved = true;
            if(editorModels.length === 0) {
                closeAllModalsAtOnce();
                return false;
            }
        },
        autofocus: false,
        allowMultiple: true
    });
    $('#startupModal').modal({
        closable: false,
        dimmerSettings: {
            closable: false
        },
        onHide: () => {
            if(editorModels.length === 0) {
                // Workaround to ensure clicking the dimmer doesn't close the startupModal
                $('#newModal').modal('hide');
                $('#openModal').modal('hide');
                $('#manageItemsModal').modal('hide');
                return false;
            }
        },
        autofocus: false,
        allowMultiple: true
    });
    $('#manageItemsModal').modal({
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetManageItemsModalState');
        },
        onApprove: () => {
            if(editorModels.length === 0) {
                closeAllModalsAtOnce();
                return false;
            }
        },
        autofocus: false,
        allowMultiple: true
    });
    $('#manageAccountModal').modal({
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetManageAccountModalState');
        },
        onApprove: () => {
            if(editorModels.length === 0) {
                closeAllModalsAtOnce();
                return false;
            }
        },
        autofocus: false,
        allowMultiple: true
    });
    $('#previewModal').modal({
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetPreviewModalState');
        },
        autofocus: false
    });
    $('#configureModal').modal({
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetConfigureModalState');
        },
        onApprove: () => {
            if(editorModels.length === 0) {
                closeAllModalsAtOnce();
                return false;
            }
        },
        autofocus: false,
        allowMultiple: true
    });
    $('#manageFilesModal').modal({
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetManageFilesModalState');
        },
        onApprove: () => {
            if(editorModels.length === 0) {
                closeAllModalsAtOnce();
                return false;
            }
        },
        onShow: () => {
            $('#renameButton').popup({
                popup: '#renamePopup',
                on: 'click',
                onHidden: () => {
                    DotNet.invokeMethodAsync('BlazorEditor', 'ManageFilesClearRenameValue');
                }
            });
        },
        autofocus: false,
        allowMultiple: true
    });
    
    addSubtabs();
    require.config({ paths: { 'vs': '/webwebResources/lib/monaco-editor/min/vs' } });
    require(['vs/editor/editor.main'], () => {
        showStartupDialog();
        document.addEventListener("first-tab-created", e => {
            $('#tab0').addClass('active');
            monaco.editor.setTheme('vs-dark');
            editor = monaco.editor.create(document.getElementById('monacoContainer'), {
                model: editorModels[0].models[0].model
            });
            $(window).resize(function() {
                editor.layout();
            });
            $(window).bind('keydown', function(e) {
                // https://stackoverflow.com/a/14385600/
                let key = undefined;
                let possible = [ e.key, e.keyIdentifier, e.keyCode, e.which ];

                while (key === undefined && possible.length > 0)
                {
                    key = possible.pop();
                }

                if (key && (key == '115' || key == '83' ) && (e.ctrlKey || e.metaKey) && !(e.altKey))
                {
                    e.preventDefault();
                    DotNet.invokeMethodAsync("BlazorEditor", "SaveToDb");
                    return false;
                }
                return true;
            });
            $('#loader').remove();
        })
    });
}

function addSubtabs() {
    subtabs.forEach((st, n) => {
        $('#subtabs').append($(`<a class="${(n === 0) ? "active item" : "item"}" id="subtab${n}" onclick="selectSubtab(${n})">${st}</a>`));
    });
}

function updateSubtabs(isLayout) {
    if(isLayout) {
        $('#subtab1').hide();
        $('#subtab2').hide();
    }
    else {
        $('#subtab1').show();
        $('#subtab2').show();
    }
}

function addEditorModel(modelName, htmlContents, cssContents, jsContents) {
    return editorModels.push(new editorTab(modelName, [htmlContents, cssContents, jsContents], (cssContents === undefined))) - 1;
}

function addTab(tabID) {
    $('#version').before($(`
    <a class="tab item" id="tab${tabID}" onclick="selectTab(${tabID})">
        <i class="tabClose closeActive grey times circle icon" onclick="removeTab(${tabID})"></i>
        ${editorModels[tabID].title}
    </a>
    `));
    
    if(tabID === 0) {
        document.dispatchEvent(new CustomEvent("first-tab-created"));
        $('.tabClose').removeClass("closeActive");
        updateSubtabs(editorModels[tabID].isLayout);
    }
    else {
        $('.tabClose:not(.closeActive)').addClass("closeActive");
        selectTab(tabID);
    }
}

function selectTab(tabID) {
    // When the remove tab button is clicked this is called after the tab is removed, as
    // clicking on the remove button is also clicking on the tab
    if(editorModels[tabID] !== undefined) {
        saveViewState();

        $('#tab' + currentTabID).removeClass('active');
        $('#tab' + tabID).addClass('active');
        $('#subtab' + editorModels[currentTabID].subtab).removeClass('active');
        $('#subtab' + editorModels[tabID].subtab).addClass('active');

        currentTabID = tabID;
        
        updateSubtabs(editorModels[tabID].isLayout);
        loadCurrentModel();
    }
}

function selectSubtab(tabID) {
    saveViewState();

    $('#subtab' + editorModels[currentTabID].subtab).removeClass('active');
    $('#subtab' + tabID).addClass('active');
    
    editorModels[currentTabID].subtab = tabID;
    
    loadCurrentModel();
}

function saveViewState() {
    editorModels[currentTabID].models[editorModels[currentTabID].subtab].viewState = editor.saveViewState();
}

function loadCurrentModel() {
    let newModel = editorModels[currentTabID].models[editorModels[currentTabID].subtab];

    editor.setModel(newModel.model);
    if(newModel.viewState !== undefined) {
        editor.restoreViewState(newModel.viewState);
    }

    editor.focus();
}

function getTabContents() {
    let output = [];
    editorModels.forEach((tab, tn) => {
        if(tab === undefined) {
            output.push(undefined);
        }
        else {
            output.push([]);
            tab.models.forEach((st) => {
                output[tn].push(st.model.getValue());
            });
        }
    });
    
    return output;
}

function removeTab(tabID) {
    // We can't remove the item from editorModels as that will change the ID of all of the other tabs
    
    if(currentTabID === tabID) {
        // Find a new tab to switch to
        let newTab = -1;
        for(let i = currentTabID - 1; (i >= 0) && (newTab === -1); i -= 1) {
            if(editorModels[i] !== undefined) {
                newTab = i;
            }
        }
        
        if(newTab === -1) {
            // No tabs to the left, look to the right
            for(let i = currentTabID + 1; (i < editorModels.length) && (newTab === -1); i += 1) {
                if(editorModels[i] !== undefined) {
                    newTab = i;
                }
            }
        }
        
        if(newTab === -1) {
            // Still nothing? This must be the last tab
            // Do nothing
            return;
        }
        
        selectTab(newTab);
    }
    
    // Delete the tab
    $('#tab' + tabID).remove();
    editorModels[tabID] = undefined;
    
    // If there's only one tab left, then hide the close button
    let notUndefined = 0;
    editorModels.forEach(item => {if(item !== undefined) {notUndefined += 1;}});
    if(notUndefined === 1) {
        // Only one tab left
        $('.tabClose').removeClass("closeActive");
    }
}

function showToast(title, message, showProgress, tClass) {
    $('body').toast({
        title: title,
        message: message,
        showProgress: showProgress,
        class: tClass
    });
}

function showOpenDialog() {
    $('#openModal').modal('show');
}

function showNewDialog() {
    $('#newModal').modal('show');
}

function showStartupDialog() {
    $('#startupModal').modal('show');
}

function showManageItemsDialog() {
    $('#manageItemsModal').modal('show');
}

function showManageAccountDialog() {
    $('#manageAccountModal').modal('show');
}

function showPreviewDialog() {
    $('#previewModal').modal('show');
}

function showConfigureDialog() {
    $('#configureModal').modal('show');
}

function showManageFilesDialog() {
    $('#manageFilesModal').modal('show');
}

function showDeployGitHubDialog() {
    // Not sure why this needs to be down here but it does
    $('#deployGitHubModal').modal({
        onHide: () => {
            return ($('#deployGitHubModal > .actions').length > 0);
        },
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetDeployGitHubModalState');
        },
        autofocus: false
    }).modal('show');
}

function showDeployFSDialog() {
    // Not sure why this needs to be down here but it does
    $('#deployFSModal').modal({
        onHide: () => {
            return ($('#deployFSModal > .actions').length > 0);
        },
        onHidden: () => {
            DotNet.invokeMethodAsync('BlazorEditor', 'ResetDeployFSModalState');
        },
        autofocus: false
    }).modal('show');
}

function resetNewItemDropdown() {
    $('#newItemTypeDropdown').dropdown('restore defaults');
}

function resetPreviewModalDropdown() {
    $('#previewModalDropdown').dropdown('restore defaults');
}

function doTransition(element) {
    $('#' + element).transition('toggle');
}

function setPreviewIframeContent(content) {
    let frame = document.getElementById("previewIframe");
    frame.src = "about:blank";
    frame.contentWindow.document.open();
    frame.contentWindow.document.write(content);
    frame.contentWindow.document.close();
}

function redirectPage(url) {
    // Disable blazor reconnect modal
    disableBlazorReconnect();
    window.location = url;
}

function disableBlazorReconnect() {
    $('head').append($('<style>#components-reconnect-modal { display: none !important; }</style>'));
}

function saveAsFile(filename, bytesBase64) {
    let link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}

function resetConfigureTabs() {
    $('#configureModalTabs').tab('change tab', 'general');
    let tabs = $('#configureModalTabs .item');
    $.makeArray(tabs).forEach((item, index) => {
        if(index === 0) {
            $(item).addClass('active');
        }
        else {
            $(item).removeClass('active');
        }
    })
}

function setOpenModalPreviewContents(contents) {
    if(document.getElementById("openModalCodePreview")) {
        $('#openModalCodePreview').removeClass('hljs').html("").text(contents);
        if(contents !== "") {
            hljs.highlightBlock(document.getElementById("openModalCodePreview"));
        }
    }
    else {
        // Set up mutation observer to watch for the element and add the contents when it appears
        const targetNode = document.getElementById("preview");
        const config = { attributes: false, childList: true, subtree: false };
        const callback = (mutationsList, observer) => {
            setOpenModalPreviewContents(contents);
            observer.disconnect();
        }
        const observer = new MutationObserver(callback);
        observer.observe(targetNode, config);
    }
}

function uploadFile(fileDirectory) {
    let file = $('#fileUpload')[0].files[0];
    if (file === undefined) return;
    $('#uploadFileButton').addClass("loading");
    let fd = new FormData();
    fd.append('FileUpload', file);
    fd.append('FileDirectory', fileDirectory);
    fd.append('__RequestVerificationToken', $("input[name='__RequestVerificationToken']").val());
    $.ajax({
        url: '/UploadFile',
        type: 'post',
        data: fd,
        contentType: false,
        processData: false,
        success: response => {
            let uploadButton = $('#uploadFileButton');
            uploadButton.removeClass('loading');
            if(response === 0) {
                // Error
                uploadButton.text("Error");
            }
            else {
                // Success
                DotNet.invokeMethodAsync("BlazorEditor", "ManageFilesRefresh");
            }
        }
    });
}

function hideRenamePopup() {
    $('#renameButton').popup('hide');
}
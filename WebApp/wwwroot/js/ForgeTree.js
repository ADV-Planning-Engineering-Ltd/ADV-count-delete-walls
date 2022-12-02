/////////////////////////////////////////////////////////////////////
// Copyright 2022 Autodesk Inc
// Written by Develope Advocacy and Support
//

// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

$(document).ready(function () {
  prepareAppBucketTree();
  $('#refreshBuckets').click(function () {
    $('#appBuckets').jstree(true).refresh();
  });

  $('#createNewBucket').click(function () {
    createNewBucket();
  });

  $('#createBucketModal').on('shown.bs.modal', function () {
    $("#newBucketKey").focus();
  })
});

function createNewBucket() {
  var bucketKey = $('#newBucketKey').val();
  var policyKey = $('#newBucketPolicyKey').val();
  jQuery.post({
    url: '/api/apsoss/buckets',
    contentType: 'application/json',
    data: JSON.stringify({ 'bucketKey': bucketKey, 'policyKey': policyKey }),
    success: function (res) {
      $('#appBuckets').jstree(true).refresh();
      $('#createBucketModal').modal('toggle');
    },
    error: function (err) {
      if (err.status === 409)
        alert('Bucket already exists - 409: Duplicated')
      console.log(err);
    }
  });
}

function prepareAppBucketTree() {
  $('#appBuckets').jstree({
    'core': {
      'themes': { "icons": true },
      'data': {
        "url": '/api/apsoss/buckets',
        "dataType": "json",
        'multiple': false,
        "data": function (node) {
          return { "id": node.id };
        }
      }
    },
    'types': {
      'default': {
        'icon': 'glyphicon glyphicon-question-sign'
      },
      '#': {
        'icon': 'glyphicon glyphicon-cloud'
      },
      'bucket': {
        'icon': 'glyphicon glyphicon-folder-open'
      },
      'object': {
        'icon': 'glyphicon glyphicon-file'
      }
    },
    "plugins": ["types", "state", "sort", "contextmenu"],
    contextmenu: { items: autodeskCustomMenu }
  }).on('loaded.jstree', function () {
    $('#appBuckets').jstree('open_all');
  }).bind("activate_node.jstree", function (evt, data) {
    if (data !== null && data.node !== null && data.node.type === 'object') {
      if (data.node.text.indexOf('.txt') > 0) return;
      resetCount();
      $("#apsViewer").empty();
      var urn = data.node.id;
      getAPSToken(function (access_token) {
        jQuery.ajax({
          url: 'https://developer.api.autodesk.com/modelderivative/v2/designdata/' + urn + '/manifest',
          headers: { 'Authorization': 'Bearer ' + access_token },
          success: function (res) {
            if (res.status === 'success') launchViewer(urn);
            else $("#apsViewer").html('The translation job still running: ' + res.progress + '. Please try again in a moment.');
          },
          error: function (err) {
            var msgButton = 'This file is not translated yet! ' +
              '<button class="btn btn-xs btn-info" onclick="translateObject()"><span class="glyphicon glyphicon-eye-open"></span> ' +
              'Start translation</button>'
            $("#apsViewer").html(msgButton);
          }
        });
      })
    }
  });
}

function autodeskCustomMenu(autodeskNode) {
  var items;

  switch (autodeskNode.type) {
    case "bucket":
      items = {
        uploadFile: {
          label: "Upload file",
          action: function () {
            var treeNode = $('#appBuckets').jstree(true).get_selected(true)[0];
            uploadFile(treeNode);
          },
          icon: 'glyphicon glyphicon-cloud-upload'
        }
      };
      break;
    case "object":
      items = {
        translateFile: {
          label: "Translate",
          action: function () {
            var treeNode = $('#appBuckets').jstree(true).get_selected(true)[0];
            translateObject(treeNode);
          },
          icon: 'glyphicon glyphicon-eye-open'
        }
      };
      break;
  }

  return items;
}

function uploadFile(node) {
  $('#hiddenUploadField').click();
  $('#hiddenUploadField').change(function () {
    if (this.files.length === 0) return;
    var file = this.files[0];
    switch (node.type) {
      case 'bucket':
        var formData = new FormData();
        formData.append('inputFile', file);
        formData.append('bucketKey', node.id);
        $.ajax({
          url: '/api/apsoss/objects',
          data: formData,
          processData: false,
          contentType: false,
          type: 'POST',
          success: function (data) {
            $('#appBuckets').jstree(true).refresh_node(node);
          }
        });
        break;
    }
  });
}

function translateObject(node) {
  $("#apsViewer").empty();
    if (node === null || node === undefined)  node = $('#appBuckets').jstree(true).get_selected(true)[0];

    startConnection(function () {
        var bucketKey = node.parents[0];
        var objectKey = node.id;
        jQuery.post({
            url: '/api/apsmodelderivative/jobs',
            contentType: 'application/json',
            data: JSON.stringify({ 'bucketKey': bucketKey, 'objectName': objectKey, 'connectionId': connectionId }),
            success: function (res) {
                $("#apsViewer").html('Translation started! Model will load when ready..');
            }
        });
  });
}

function resetCount() {
  $('#walls')[0].innerText = '';
  $('#floors')[0].innerText = '';
  $('#doors')[0].innerText = '';
  $('#windows')[0].innerText = '';
}
window.companyLocal = localforage.createInstance({
    name: "CompanyList"
});

 window.providerLocal = localforage.createInstance({
    name: "ProviderList"
});

window.authCodesLocal = localforage.createInstance({
    name: "AuthCodes"
});

window.StaffListDependantLocal = localforage.createInstance({
    name: "StaffListDependant"
});


window.servicetariffLocal = localforage.createInstance({
    name: "ServiceTariff"
});

window.drugtariffLocal = localforage.createInstance({
    name: "DrugTariffList"
});

window.VettingProtocolLocal = localforage.createInstance({
    name: "VettingProtocolDb"
});


window.providerportalclaim = localforage.createInstance({
    name: "providerportalclaimDB"
});

window.claimtopush = localforage.createInstance({
    name: "claimtopushclaimDB"
});




$(function () {
    
   
    window.globalNewIndex = $('#servicebilllist tr').length;
   
   
    window.DglobalNewIndex = $('#drugbilllist tr').length;
    $('#servicebillCount').val(window.globalNewIndex);
    $('#drugbillCount').val(window.DglobalNewIndex);

    
    $('body').on('click', '.modal-link', function (e) {
        e.preventDefault();

        $(this).attr('data-target', '#modal-container');
        //$(this).attr('data-toggle', 'modal');
        //target to pen

        var target = $(this).attr("href");
        //alert(target);

        //$('#modal-container').modal({
        //    keyboard: false,
        //    remote: target
        //}).show();
                 

        $("#modal - body").empty();

        $("#modal - body").append("<p>Loading...</p>");
        // load the url and show modal on success
        $("#modal-container .modal-body").load(target, function () {
                    
            $("#modal-container").modal("show");
            //alert('loaded');
        });

    });

    $('body').on('click', '.modal-close-btn', function () {

        $('modal-container').modal('hide');

    });
    $('#modal-container').on('shown.bs.modal', function () {
        //When the modal is displayed
        //activate choosen
        $('.chzn-select', this).chosen('destroy').chosen();
        $('#benefitlistforplan').DataTable().ajax.reload();
                 
        //Date picker
        $('#datepicker').datepicker({
            autoclose: true
           
        });
        //Date picker
        $('#datepicker2').datepicker({
            autoclose: true
        });
                
    });

    $('#modal-container').on('hidden.bs.modal', function () {

        $(this).removeData('bs-modal');
    });
});
//Add category
function addCategory(id) {
    var $form = $('#categoryform');
    var dataToPost = $form.serialize();
         
    $.post("/Tariff/addcategory", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $.getJSON("/Tariff/GetCategoryJson/?id=" +  id, function (data) {
                var items = [];
                //empty ui
                if(id==0) {
                    $("#categorylist").empty();
                    $.each(data, function (key, val) {

                        $("#categorylist").append("<li><a href='#'>" + val.Name + "</a> </li>");


                    });

                }else {
                    $("#categorylist2").empty();
                    $.each(data, function (key, val) {

                        $("#categorylist2").append("<li><a href='#'>" + val.Name + "</a> </li>");


                    });

                }
                        
            });
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          
function addDrug() {
    var $form = $('#drugform');
    var dataToPost = $form.serialize();

    $.post("/Tariff/AddDrug", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#pricelist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
function EditDrug() {
    var $form = $('#drugform');
    var dataToPost = $form.serialize();

    $.post("/Tariff/EditDrug", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#pricelist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
function removeRow(el) {
    $(el).parents("tr").remove();
    CalculateTotalDrug();
    CalculateTotalService();
    //CalculateTotalDrug();
}

function editRow(el) {
    var id = $(el).parents("tr").attr('Id');
   var track= id.split("_")[1];
   
    //fill up the boxes
   $('#drugItemDescription').val($('#drugbill' + track + 'ItemDescription').val());

  
    //$("#serviceItemDescriptionList").
  
    //check if its zero
   $('#drugunit').val($('#drugbill' + track + 'Unit').val());
   $('#drugrate').val($('#drugbill' + track + 'Rate').val());
   $('#drugamount').val($('#drugbill' + track + 'amount').val());

   var drugidd = $('#drugbill' + track + 'itemID').val();
   if (drugidd == null) {
       // your code here.


       drugidd = "0";
   }

    //set the selection if its
   if (!isInt(drugidd)) {
       $("#drugItemDescriptionList").val($('#drugbill' + track + 'ItemDescription').val());
       $("#drugItemDescriptionList").trigger('chosen:updated');
       
   } else {
       if (drugidd === "0") {

           var exists = false;
           $('#drugItemDescriptionList option').each(function () {
               if (this.value == $('#drugbill' + track + 'ItemDescription').val()) {
                   exists = true;

               }
           });

           if (!exists) {
               //add to the items
               $("#drugItemDescriptionList").append('<option value="' + $('#drugbill' + track + 'ItemDescription').val() + '">' + $('#drugbill' + track + 'ItemDescription').val() + '</option>');
               $("#drugItemDescriptionList").val($('#drugbill' + track + 'ItemDescription').val());
               $("#drugItemDescriptionList").trigger('chosen:updated');

           } else {
               $("#drugItemDescriptionList").val($('#drugbill' + track + 'ItemDescription').val());
               $("#drugItemDescriptionList").trigger('chosen:updated');
           }

       }else{
           $("#drugItemDescriptionList").val(drugidd);
           $("#drugItemDescriptionList").trigger('chosen:updated');
       }

       

      
   }

   $('#drugEditID').val(track);
   $('#drugupdatebtn').removeClass('hidden');
   $('#drugcancelbtn').removeClass('hidden');

    //set the hidden id too
   $('#drugbill_hiddenID').val(drugidd);

   $('#drugaddbtn').addClass("hidden");

    $('#druginputrow').focus();
    //show edit button hide add button
    

    //CalculateTotalDrug();
}
function isInt(value) {
    if (isNaN(value)) {
        return false;
    }
    var x = parseFloat(value);
    return (x | 0) === x;
}
function editRow2(el) {
    var id = $(el).parents("tr").attr('Id');
    var track = id.split("_")[1];

    //fill up the boxes
    $('#serviceItemDescription').val($('#servicebill' + track + 'ItemDescription').val());
    $('#serviceduration').val($('#servicebill' + track + 'Duration').val());
    $('#servicerate').val($('#servicebill' + track + 'Rate').val());
    $('#serviceamount').val($('#servicebill' + track + 'amount').val());

    $('#serviceEditID').val(track);
    $('#serviceupdatebtn').removeClass('hidden');
    $('#servicecancelbtn').removeClass('hidden');
    var serviceidd = $('#servicebill' + track + 'itemID').val();
    //alert(serviceidd);
    //set the selection if its
    //newly added

   
    if (serviceidd == null) {
        // your code here.
       

        serviceidd = "0";
    }
    
    if (!isInt(serviceidd)) {

        
       
        $("#serviceItemDescriptionList").val($('#servicebill' + track + 'ItemDescription').val());
        //$("#serviceItemDescriptionList").text($('#servicebill' + track + 'ItemDescription').val());
        $("#serviceItemDescriptionList").trigger('chosen:updated');

    } else {

        if (serviceidd === "0"  ) {

            var exists = false;
            $('#serviceItemDescriptionList option').each(function () {
                if (this.value == $('#servicebill' + track + 'ItemDescription').val()) {
                    exists = true;
                  
                }
            });
            //alert(exists);
            if (!exists) {
                
                //add to the items
                $("#serviceItemDescriptionList").append('<option value="' + $('#servicebill' + track + 'ItemDescription').val() + '">' + $('#servicebill' + track + 'ItemDescription').val() + '</option>');
                $("#serviceItemDescriptionList").val($('#servicebill' + track + 'ItemDescription').val());
                $("#serviceItemDescriptionList").trigger('chosen:updated');
                
            } else {
                $("#serviceItemDescriptionList").val($('#servicebill' + track + 'ItemDescription').val());
                $("#serviceItemDescriptionList").trigger('chosen:updated');
            }
            
      
             
        } else {
            $("#serviceItemDescriptionList").val(serviceidd);
            $("#serviceItemDescriptionList").trigger('chosen:updated');
        }
       
    }


    //set the hidden id too
    $('#servicebill_hiddenID').val(serviceidd);


    $('#serviceaddbtn').addClass("hidden");

    $('#serviceItemDescription').focus();
    //show edit button hide add button

    $('html, body').animate({
        scrollTop: $("#serviceItemDescription").offset().bottom
    }, 1000);
    //CalculateTotalDrug();
}
function cancelEditRow(el) {

   

    $('#drugupdatebtn').addClass('hidden');
        $('#drugcancelbtn').addClass('hidden');

        $('#drugaddbtn').removeClass("hidden");


    //clear text
        $("#drugItemDescriptionList").val(-1);
        $("#drugItemDescriptionList").trigger('chosen:updated');
        $('#drugItemDescription').val('');
        $('#drugItemDescriptionlist').val('');
        $('#drugunit').val('');
        $('#drugrate').val('');
        $('#drugamount').val('');
        $('#drugbill_hiddenID').val('');
        //ResetSelecteable();

        $('#drugEditID').val('');
 
}

function cancelServiceEditRow(el) {

 

    $('#serviceupdatebtn').addClass('hidden');
    $('#servicecancelbtn').addClass('hidden');

    $('#serviceaddbtn').removeClass("hidden");


    //clear text
    $("#serviceItemDescriptionList").val(-1);
    $("#serviceItemDescriptionList").trigger('chosen:updated');
    $('#serviceItemDescription').val('');
    $('#serviceduration').val('');
    $('#servicerate').val('');
    $('#serviceamount').val('');
    $('#servicebill_hiddenID').val('');
    //ResetSelecteable();

    $('#serviceEditID').val('');

}
function DoAddServiceRow() {
    //$('#servicebill0ItemDescription').attr('id', 'henryford');

    //validate textbox
   

    if ( $("#serviceItemDescriptionList option:selected").text() == "") {
        
        toastr["error"]("Service Description cannot be left blank.", "Input Error");
        return;
    }
    if (!!$('#serviceduration').val().length < 1) {
        toastr["error"]("Service Duration cannot be left blank.", "Input Error");
        return;
    }
    if (!!$('#servicerate').val().length < 1) {
        toastr["error"]("Service Rate cannot be left blank.", "Input Error");
        return;
    }
    if (!!$('#serviceamount').val().length < 1) {
        toastr["error"]("Service Amount cannot be left blank.", "Input Error");
        return;
    }

  

    if (isNumber($('#servicerate').val()) == false) {
        toastr["error"]("Service Rate must be numeric.", "Input Error");
        return;
    }

    if (isNumber($('#serviceamount').val()) ==false) {
        toastr["error"]("Service Amount must be numeric.", "Input Error");
        return;
    }

    if (!!$('#serviceEditID').val()) {

        //update so update the guy
  
        $('#servicebill' + ($('#serviceEditID').val()) + 'ItemDescription').val($("#serviceItemDescriptionList option:selected").text());
        $('#servicebill' + ($('#serviceEditID').val()) + 'Duration').val($('#serviceduration').val());
        $('#servicebill' + ($('#serviceEditID').val()) + 'Rate').val($('#servicerate').val());
        $('#servicebill' + ($('#serviceEditID').val()) + 'amount').val($('#serviceamount').val());
        $('#servicebill' + ($('#serviceEditID').val()) + 'itemID').val($('#servicebill_hiddenID').val());

        $('#serviceupdatebtn').addClass('hidden');
        $('#servicecancelbtn').addClass('hidden');
        $('#serviceaddbtn').removeClass("hidden");
    } else {
        $('#servicebilllist tbody>tr:first').clone(true).insertAfter('#servicebilllist tbody>tr:last');

        //var newIndex = globalNewIndex + 1;
        var changeIds = function (i, val) {
            var newIndex = globalNewIndex + 1;
            return val.replace("0", newIndex);
        }

        //servicebill0ItemDescription
        //$('#servicebill.entry0.ItemDescription').attr('name', 'henryford');
        $('#servicebilllist tbody>tr:last input').attr('id', changeIds);
        $('#servicebilllist tbody>tr:last input').attr('name', changeIds);


        $('#servicebilllist tbody>tr:last').attr('id', changeIds);
        $('#servicebilllist tbody>tr:last').attr('name', changeIds);
        //alert('hih');
        //fill the information
        $('#servicebill' + (window.globalNewIndex + 1) + 'ItemDescription').val($("#serviceItemDescriptionList option:selected").text());
        $('#servicebill' + (window.globalNewIndex + 1) + 'Duration').val($('#serviceduration').val());
        $('#servicebill' + (window.globalNewIndex + 1) + 'Rate').val($('#servicerate').val());
        $('#servicebill' + (window.globalNewIndex + 1) + 'amount').val($('#serviceamount').val());
        $('#servicebill' + (window.globalNewIndex + 1) + 'itemID').val($('#servicebill_hiddenID').val());

        //unhide
        $('#servicebilllist tbody>tr:last').removeClass('hidden');
        window.globalNewIndex = window.globalNewIndex + 1;
        $('#servicebillCount').val(window.globalNewIndex);
    }

   


    //Reset the values
    $("#serviceItemDescriptionList").val(-1);
    $("#serviceItemDescriptionList").trigger('chosen:updated');
    $('#serviceItemDescription').val('');
    $('#serviceduration').val('');
    $('#servicerate').val('');
    $('#serviceamount').val('');
    $('#servicebill_hiddenID').val('');
    //ResetSelecteable();
 
    $('#serviceEditID').val('');


   
   

    CalculateTotalService();
    return false;
}
function isNumber(n) {
    'use strict';

    var myNumeral = numeral(n);
   
    n = n.replace(/\./g, '').replace(',', '.');
    !isNaN(parseFloat(myNumeral.value())) && isFinite(myNumeral.value());

    //alert(myNumeral.value());
}
function DoAddDrugRow() {
    //$('#servicebill0ItemDescription').attr('id', 'henryford');

    //validate textbox


    if ($("#drugItemDescriptionList option:selected").text() =="") {

        toastr["error"]("Drug Description cannot be left blank.", "Input Error");
        return;
    }
    if (!!$('#drugunit').val().length < 1) {
        toastr["error"]("Drug Unit cannot be left blank.", "Input Error");
        return;
    }
    if (!!$('#drugrate').val().length < 1) {
        toastr["error"]("Drug Rate cannot be left blank.", "Input Error");
        return;
    }
    if (!!$('#drugamount').val().length < 1) {
        toastr["error"]("Drug Amount cannot be left blank.", "Input Error");
        return;
    }
    if (isNumber($('#drugrate').val()) == false) {
        toastr["error"]("Drug Rate must be numeric.", "Input Error");
        return;
    }

    if (isNumber($('#drugamount').val()) == false) {
        toastr["error"]("Drug Amount must be numeric.", "Input Error");
        return;
    }

    if (!!$('#drugEditID').val()) {
        //update so update the guy
        $('#drugbill' + ($('#drugEditID').val()) + 'ItemDescription').val($("#drugItemDescriptionList option:selected").text());
        $('#drugbill' + ($('#drugEditID').val()) + 'Unit').val($('#drugunit').val());
        $('#drugbill' + ($('#drugEditID').val()) + 'Rate').val($('#drugrate').val());
        $('#drugbill' + ($('#drugEditID').val()) + 'amount').val($('#drugamount').val());
        $('#drugbill' + ($('#drugEditID').val()) + 'itemID').val($('#drugbill_hiddenID').val());

        $('#drugupdatebtn').addClass('hidden');
        $('#drugcancelbtn').addClass('hidden');
        $('#drugaddbtn').removeClass("hidden");
    } else {
        $('#drugbilllist tbody>tr:first').clone(true).insertAfter('#drugbilllist tbody>tr:last');

        //var newIndex = globalNewIndex + 1;
        var changeIds = function (i, val) {
            var newIndex = DglobalNewIndex + 1;
            return val.replace("0", newIndex);
        }

        //servicebill0ItemDescription
        //$('#servicebill.entry0.ItemDescription').attr('name', 'henryford');
        $('#drugbilllist tbody>tr:last input').attr('id', changeIds);
        $('#drugbilllist tbody>tr:last input').attr('name', changeIds);

        $('#drugbilllist tbody>tr:last').attr('id', changeIds);
        $('#drugbilllist tbody>tr:last').attr('name', changeIds);
        //alert('hih');
        //fill the information
        $('#drugbill' + (window.DglobalNewIndex + 1) + 'ItemDescription').val($('#drugItemDescriptionList option:selected').text());
        $('#drugbill' + (window.DglobalNewIndex + 1) + 'Unit').val($('#drugunit').val());
        $('#drugbill' + (window.DglobalNewIndex + 1) + 'Rate').val($('#drugrate').val());
        $('#drugbill' + (window.DglobalNewIndex + 1) + 'amount').val($('#drugamount').val());
        $('#drugbill' + (window.DglobalNewIndex + 1) + 'itemID').val($('#drugbill_hiddenID').val());


        //unhide
        $('#drugbilllist tbody>tr:last').removeClass('hidden');
        window.DglobalNewIndex = window.DglobalNewIndex + 1;
        $('#drugbillCount').val(window.DglobalNewIndex);
    }

   
    //Reset the values
    $("#drugItemDescriptionList").val(-1);
    $("#drugItemDescriptionList").trigger('chosen:updated');
    $('#drugItemDescription').val('');
 
    $('#drugunit').val('');
    $('#drugrate').val('');
    $('#drugamount').val('');
    $('#drugbill_hiddenID').val('');
    //ResetSelecteable();

    $('#drugEditID').val('');
    //recalculate the total
    CalculateTotalDrug();
    return false;
}

function CalculateTotalDrug() {
    var sum = 0;
    $('.drugamount').each(function () {
        var myNumeral = numeral($(this).val());
        //console.log($(this).val());
        sum += parseFloat(myNumeral.value());  // Or this.innerHTML, this.innerText

        //alert(sum);
    });

    $('#drugbilltotal').val('₦ ' + String(sum.toFixed(2)));

    $('#drugbilltotal').formatCurrency();
}


function CalculateTotalService() {
    var sum = 0;
    $('.serviceamount').each(function () {
        var myNumeral = numeral($(this).val());
        //console.log($(this).val());
        sum += parseFloat(myNumeral.value());  // Or this.innerHTML, this.innerText

        //alert(sum);
    });

    $('#servicebilltotal').val('₦ ' + String(sum.toFixed(2)));
    $('#servicebilltotal').formatCurrency();
}
function DeleteDrug() {
    var $form = $('#drugform');
    var dataToPost = $form.serialize();

    $.post("/Tariff/DeleteDrug", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#pricelist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          
//Add service
function addService() {
    var $form = $('#serviceform');
    var dataToPost = $form.serialize();

    $.post("/Tariff/AddService", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            toastr["success"]("the record was added successfully.", "Added");
            $('#servicepricelist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          

//delete service
function EditService() {
    var $form = $('#serviceform');
    var dataToPost = $form.serialize();

    $.post("/Tariff/EditService", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            toastr["success"]("the record was updated successfully.", "Updated");
            $('#servicepricelist').DataTable().ajax.reload();
            //toast to the update.
           
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          
//delete service
function DeleteService() {
    var $form = $('#serviceform');
    var dataToPost = $form.serialize();

    $.post("/Tariff/DeleteService", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            toastr["success"]("the record was deleted successfully.", "Deleted");
            $('#servicepricelist').DataTable().ajax.reload();
            

        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
//format number
FormatPrice = function (n, x, s, c,value) {
    var re = '\\d(?=(\\d{' + (x || 3) + '})+' + (n > 0 ? '\\D' : '$') + ')',
        num = value.toFixed(Math.max(0, ~~n));

    return (c ? num.replace('.', c) : num).replace(new RegExp(re, 'g'), '$&' + (s || ','));
};

        
//Add Benefit category
//Add category
function addBenefitCategory() {
    var $form = $('#categoryform');
    var dataToPost = $form.serialize();

    $.post("/Company/addcategory", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $.getJSON("/Company/GetCategoryJson", function (data) {
                var items = [];
                //empty ui
                         
                $("#categorylist").empty();
                $.each(data, function (key, val) {
                    $("#categorylist").append("<li><a class='modal-link' href='../Company/DeleteCategory?id=" + val.Id + "'>" + val.Name + "</a>  </li>");


                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                


                });

                        

            });
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          
function addBenefit() {
    var $form = $('#Benefitform');
    var dataToPost = $form.serialize();

    $.post("/Company/AddBenefit", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          
function EditBenefit() {
    var $form = $('#Benefitform');
    var dataToPost = $form.serialize();

    $.post("/Company/EditBenefit", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}

function DeleteBenefit() {
    var $form = $('#benefitform');
    var dataToPost = $form.serialize();

    $.post("/Company/DeleteBenefit", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          
function DeleteBenefitCategory() {
    var $form = $('#categoryform');
    var dataToPost = $form.serialize();

    $.post("/Company/deletecategory", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $.getJSON("/Company/GetCategoryJson", function (data) {
                var items = [];
                //empty ui

                $("#categorylist").empty();
                $.each(data, function (key, val) {

                    //Refresh list
                    $("#categorylist").append("<li><a class='modal-link' href='../Company/DeleteCategory?id=" + val.Id + "'>" + val.Name + "</a>  </li>");

                    $('#benefitlist').DataTable().ajax.reload();

                
                });



            });
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          


//add company plan
function addCompanyPlan() {
    var $form = $('#CompanyPlanform');
    var dataToPost = $form.serialize();
    if ($('#PlanType').val() == "-1") {
        $('#PlanType').notify("Kindly select a plan type.");
        $('#PlanType').focus();
        return false;
    }
    
    if ($('#Planfriendlyname').val() == "") {
        $('#Planfriendlyname').notify("Kindly enter plan name.");
        $('#Planfriendlyname').focus();
        return false;
    }
    $.post("/Company-list/AddCompanyPlan", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#companyplanlist').DataTable().ajax.reload();
            toastr["success"]("NEw company plan Added successfully.", "Added");
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
            toastr["error"]("There was a problem adding company plan.", "error");
        });
}
          
//add benefits to plan
function AddbenefitToPlan(planid, benefitid) {
          
    var $form = $('#CompanyPlanform');
    var dataToPost = { planid: planid, benefitid: benefitid };

    $.post("/Company/AddBenefitToPlan", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            //$('#modal-container').modal('hide');
            $('#allbenefitlist').DataTable().ajax.reload();
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
function removebenefitToPlan(benefitid) {

    var $form = $('#CompanyPlanform');
    var dataToPost = {  benefitid: benefitid };

    $.post("/Company/RemoveBenefitToPlan", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            //$('#modal-container').modal('hide');
            $('#allbenefitlist').DataTable().ajax.reload();
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          
function EditBenefitLimit() {
    var $form = $('#BenefitLimitform');
    var dataToPost = $form.serialize();

    $.post("/Company/EditBenefitLimit", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
          
//Dafult plan Section
function AdddefaultbenefitToPlan(planid, benefitid) {

            
    var dataToPost = { planid: planid, benefitid: benefitid };

    $.post("/Company/AddDefaultBenefitToPlan", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            //$('#modal-container').modal('hide');
            $('#allbenefitlist').DataTable().ajax.reload();
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
function removebenefitfromPlan(benefitid) {

         
    var dataToPost = { benefitid: benefitid };

    $.post("/Company/RemoveBenefitTfromPlan", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            //$('#modal-container').modal('hide');
            $('#allbenefitlist').DataTable().ajax.reload();
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}

function EditPlanBenefitLimit() {
    var $form = $('#BenefitLimitform');
    var dataToPost = $form.serialize();

    $.post("/Company/EditPlanBenefitLimit", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            $('#benefitlist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
//add Staff
function addStaff() {
    
    if($('#CompanySubsidiary').val() < 1) {
        alert("Select Subsidiary.");
        return;
    }
    var $form = $('#Staffform');
    var dataToPost = $form.serialize();

    $.post("/Company/AddStaff", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container

         
            $('#modal-container').modal('hide');


            if (response.Id == "0") {

         
            toastr["success"]("the record was Added successfully.", "Added");
            $('#stafflist').DataTable().ajax.reload();
            $('#modal-container').modal('hide');
            }
            else {
                toastr["error"](response.Name, "error");
            }
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
            alert('There was an error adding new staff.Please try again.')
        });
}
function EditStaff() {
    var $form = $('#Staffform');
    var dataToPost = $form.serialize();

    $.post("/Company/EditStaff", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            toastr["success"]("the record was updated successfully.", "Updated");
            $('#stafflist').DataTable().ajax.reload();
                      

        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}
//function SelectChange(e)

//{            
//    $.getJSON("GetPlans/" + $(e).val(), function (data) {

//        $("#Planid").empty();

//        $.each(data, function () {
//            $("#Planid").append($("<option  />").val(this.Id).text(this.Planfriendlyname));
//        });

//    });
             
//}
function SelectChange(e)

{

 
    $.getJSON("../Company/GetPlans/" + $(e).val(), function (data) {

        $("#Planid").empty();
       
        //alert('removed');
        $.each(data, function () {
            $("#Planid").append($("<option  />").val(this.Id).text(this.Planfriendlyname));
        });
        $('.chzn-select').trigger("chosen:updated");
       
        
    });
    
    
    //get the company subsidiary
    $.getJSON("../Company/GetSubsidiary/" + $(e).val(), function (data) {
      
        $("#CompanySubsidiary").empty();
      
        //alert('removed');
        $.each(data, function () {
            $("#CompanySubsidiary").append($("<option  />").val(this.Id).text(this.Subsidaryname));
        });
       


    });
             
}

function SelectChangeSub(e) {
    $.getJSON("GetPlans/" + $(e).val(), function (data) {

        $("#Planid").empty();

        //alert('removed');
        $.each(data, function () {
            $("#Planid").append($("<option  />").val(this.Id).text(this.Planfriendlyname));
        });
        $('.chzn-select').trigger("chosen:updated");


    });


    //get the company subsidiary
    $.getJSON("GetSubsidiary/" + $(e).val(), function (data) {

        $("#CompanySubsidiary").empty();

        //alert('removed');
        $.each(data, function () {
            $("#CompanySubsidiary").append($("<option  />").val(this.Id).text(this.Subsidaryname));
        });



    });

}
function DeleteStaff() {
    var $form = $('#Staffform');
    var dataToPost = $form.serialize();

    $.post("/Company/DeleteStaff", dataToPost)
        .done(function (response, status, jqxhr) {
           
            $('#modal-container').modal('hide');
            toastr["success"]("the record was deleted successfully.", "Deleted");
            $('#stafflist').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
    
 
}
         

//add Subscription
function addSubscription() {
    var $form = $('#Subscriptionform');
    var dataToPost = $form.serialize();

    $.post("/Company/AddSubscription", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            toastr["success"]("the subscription was Added successfully,it will have to be approved.", "Added");
            $('#subscriptionTable').DataTable().ajax.reload();
            $('#pendingsubscriptionTable').DataTable().ajax.reload();

            $.getJSON("/Company/GetSubscriptionssmmaryJson", function (data) {
                var items = [];
                //empty ui
              
                $("#categorylist").empty();
                $.each(data, function (key, val) {

                    //$("#categorylist").append("<li><a href='#'>" + val.Name + "</a> </li>");
                    $("#companycount").text(val.companycount);
                    $("#companywithsubcount").text(val.companywithsub);
                    $("#companywithnosubcount").text(val.companywithnosub);
                    //alert(val.companycount);
                    //alert(val.companywithsub);
                    //alert(val.companywithnosub);
                        
                });

              

            });
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}

function editSubscription() {
    var $form = $('#Subscriptionform');
    var dataToPost = $form.serialize();

    $.post("/Company/EditSubscription", dataToPost)
        .done(function (response, status, jqxhr) {
            // this is the "success" callback
            //Reload the list
            //Close the container
            $('#modal-container').modal('hide');
            toastr["success"]("the subscription was updated successfully.", "Updated");
            $('#subscriptionTable').DataTable().ajax.reload();
           
           

            $.getJSON("/Company/GetSubscriptionssmmaryJson", function (data) {
                var items = [];
                //empty ui

                $("#categorylist").empty();
                $.each(data, function (key, val) {

                    //$("#categorylist").append("<li><a href='#'>" + val.Name + "</a> </li>");
                    $("#companycount").text(val.companycount);
                    $("#companywithsubcount").text(val.companywithsub);
                    $("#companywithnosubcount").text(val.companywithnosub);
                    //alert(val.companycount);
                    //alert(val.companywithsub);
                    //alert(val.companywithnosub);

                });



            });
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });
}

function DeleteSubscription() {
    var $form = $('#Subscriptionform');
    var dataToPost = $form.serialize();

    $.post("/Company/DeleteSubscription", dataToPost)
        .done(function (response, status, jqxhr) {

            $('#modal-container').modal('hide');
            toastr["success"]("the record was deleted successfully.", "Deleted");
            $('#subscriptionTable').DataTable().ajax.reload();
          
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });


}
function TerminateSubscription() {
    var $form = $('#Subscriptionform');
    var dataToPost = $form.serialize();

    $.post("/Company/TerminateSubscription", dataToPost)
        .done(function (response, status, jqxhr) {

            $('#modal-container').modal('hide');
            toastr["success"]("the record was terminated successfully.", "Terminated");
            $('#subscriptionTable').DataTable().ajax.reload();

        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });


}
function ApproveSubscription() {
    var $form = $('#Subscriptionform');
    var dataToPost = $form.serialize();

    $.post("/Company/ApproveSubscription", dataToPost)
        .done(function (response, status, jqxhr) {
            SelectChangeSub
            $('#modal-container').modal('hide');
            toastr["success"]("The record was Approved successfully.", "Approved");
            $('#pendingsubscriptionTable').DataTable().ajax.reload();
            $('#subscriptionTable').DataTable().ajax.reload();

        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });


}


function DisapproveSubscription() {
    var $form = $('#Subscriptionform');
    var dataToPost = $form.serialize();

    $.post("/Company/DisapproveSubscription", dataToPost)
        .done(function (response, status, jqxhr) {

            $('#modal-container').modal('hide');
            toastr["success"]("the record was approved successfully.", "Approved");
            $('#pendingsubscriptionTable').DataTable().ajax.reload();
            $('#subscriptionTable').DataTable().ajax.reload();
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
        });


}
function SelectChangeSubsidiary(e) {
 
    //$.getJSON("GetSubsidiary/?id=" + $(this).val(), function (data) {

    //    $("#Subsidiary").empty();
    //    $("#Subsidiary").append($("<option />").val(-1).text('All Subsidiary'));
    //    $.each(data, function () {
    //        $("#Subsidiary").append($("<option />").val(this.Id).text(this.Subsidaryname));
    //    });

    //});
    //alert('change');
    
    $.getJSON("GetSubsidiary/?id=" + $(e).val(), function (data) {

        $("#Subsidiary").empty();
         
        $("#Subsidiary").append($("<option />").val(-1).text('All Subsidiary'));
        $.each(data, function () {
            $("#Subsidiary").append($("<option />").val(this.Id).text(this.Subsidaryname));
        });

    });
    SelectChangeSub(e);
    $.getJSON("GetSubsidiary/?id=" + $(e).val(), function (data) {

        $("#SubsidiaryID").empty();

        $("#SubsidiaryID").append($("<option />").val(-1).text('All Subsidiary'));
        $.each(data, function () {
            $("#SubsidiaryID").append($("<option />").val(this.Id).text(this.Subsidaryname));
        });

    });
       
}
;
function addDependent() {
   
 
    var formData = new FormData($('#Dependentform')[0]);

    $.post("/Enrollee/AddDependent", formData, function (data) {
        alert(data);
    });

    return false;
}


function calculateDrugSum() {
    var sum = 0;
   
    //iterate through each td based on class and add the values
    $(".drugPPriceTEXT").each(function () {
        var valuee = this.value.replace(',', '');
        //add only if the value is number
        if (!isNaN(valuee) && valuee.length != 0) {
            sum += parseFloat(valuee);
        }

    });
    
    $('#drugbilltotalprocessedamount').val("₦ " + sum);
    $('#drugbilltotalprocessedamount').formatCurrency();

    var drugtotal = $('#drugbilltotalinitial').asNumber();
    

    var difference = drugtotal - sum;
    var fomated_Diff = '₦' + difference.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,");
    
  
    document.getElementById('drugbilltotalprocessedamount').title = 'Difference is ' + fomated_Diff;


}

function calculateServiceSum() {
    var sum = 0;

    //iterate through each td based on class and add the values
    $(".ServicePPAmount").each(function () {
        var valuee = this.value.replace(',', '');
        //add only if the value is number
        if (!isNaN(valuee) && valuee.length != 0) {
            sum += parseFloat(valuee);
        }

    });

    $('#servicebilltotalprocessedamount').val("₦ " + sum);
    $('#servicebilltotalprocessedamount').formatCurrency()

    var drugtotal = $('#servicebilltotalinitial').asNumber();


    var difference = drugtotal - sum;
    var fomated_Diff = '₦' + difference.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,");


    document.getElementById('servicebilltotalprocessedamount').title = 'Difference is ' + fomated_Diff;


}

//window.worker = new Worker('../Apps/Core/Content/Scripts/claimworker.js');
//window.worker.addEventListener('message', function (e) {
//    //alert(e.data);

//}, false);

//window.worker.onmessage = function (event) {
//    console.log(event);
//    alert(event.data.resp);
//};





function RunCheck() {
    localforage.iterate(function (value, key, iterationNumber) {
        //post to worker to submit
        //window.worker.postMessage({ key: key, value: value });
        var jqxhr = $.post("../ClaimsPage/SubmitClaimsForm2", value, function (data) {
            console.log('Sent to server successfully.');
            console.log(data);
            localforage.removeItem(data).then(function () {
                // Run this code once the key has been removed.
                console.log('Key is cleared!');
            }).catch(function (err) {
                // This code runs if there were any errors
                console.log(err);
            });

        })
  .done(function() {
      console.log("second success");
  })
  .fail(function() {
     
  })
  
 

    }).then(function () {
        console.log('Iteration has completed');
    }).catch(function (err) {
        // This code runs if there were any errors
        console.log(err);
    });

    setTimeout(function () { RunCheck(); }, 120000);
}

//run continous checks

RunCheck();
function RunCheckAuth() {
   
    window.authCodesLocal.iterate(function (value, key, iterationNumber) {
        console.log(value);
        //post to worker to submit
        //window.worker.postMessage({ key: key, value: value });
        //window.authCodesLocal.removeItem(key).then(function () {
        //    // Run this code once the key has been removed.
        //    console.log('Key is cleared!');
        //}).catch(function (err) {
        //    // This code runs if there were any errors
        //    console.log(err);
        //});
        //$.post("../ClaimsPage/GenerateAuthorizationCode2", value);

        var shitttt = $.post("../ClaimsPage/GenerateAuthorizationCode2", value, function (data) {
            console.log('Sent to serverffff successfully.');
            console.log(data);

            window.authCodesLocal.removeItem(data).then(function () {
                // Run this code once the key has been removed.
                console.log('Key is cleared!');
            }).catch(function (err) {
                // This code runs if there were any errors
                console.log(err);
            });

        });



        
    }).then(function () {
        console.log('Iteration authcodes has completed');
    }).catch(function (err) {
        // This code runs if there were any errors
        console.log(err);
    }); setTimeout(function () { RunCheckAuth(); }, 60000);
}

RunCheckAuth();
function QuickLoadService() {
    
    var authorizeProviders = $("#auth_provider_list");
    var authorizeCompany = $("#auth_company_list");
    $(function () {
        $('#NHISSwitch').change(function () {
            if ($(this).is(":checked")) {

                $("#auth_authorizationcode").val($("#NHISBUFF").val());
            } else {
                $("#auth_authorizationcode").val($("#NormalBUFF").val());
            }
            
        });


        $("#auth_submit").on("click", function () {
           
           
            //do validation
            
            if ($("#auth_company_list").val() === null) {
                
                $("#auth_company_list").notify("You need to select company.", "errpr");
                toastr["error"]("You need to select company", "Error");
                $("#auth_company_list").focus();
                return false;
            }

            if ($("#auth_provider_list").val() === null) {

            toastr["error"]("You need to select provider", "Error");
            $("#auth_provider_list").focus();
                return false;
            }
            
            if ($("#Whatyouauthorized").val() == '') {

                toastr["error"]("You need to enter what you authorized", "Error");
                $("#Whatyouauthorized").focus();
                return false;
            }


            var keyholder2 = 0;
            //save the form for later submition
            var formstorre = $('#auth_generateForm').serialize();

            window.authCodesLocal.length().then(function (value) {
                keyholder2 = value + 1;
                //alert(keyholder2);
                //alert(keyholder);
            });
            //alert(keyholder2.toString());
            window.authCodesLocal.setItem(keyholder2.toString(), formstorre).then(function () {
                return window.authCodesLocal.getItem(keyholder2.toString());
            }).then(function (value) {
                toastr["success"]("Saved Successfully.", "Authorized");
                $('#auth_generateForm').trigger("reset");
                $('#auth_policynumber').val('');
                
                $('#authorizationModal').modal('hide');
            }).catch(function (err) {
                // we got an error
                console.log(err);
            });
        });
        $('#authorizationModal').on('shown.bs.modal',
            function (e) {

               




                // do something...
                $("#authorizationModal").delegate("#auth_enrolleeName", "focusin", function () {
                    checkpolicy($("#auth_policynumber").val());
                    console.log('focusin');
                });
                $("#auth_admissiondate").inputmask("dd/mm/yyyy", { "placeholder": "dd/mm/yyyy" });

                var guidd1 = guidCus(0);
                var guidd2 = guidCus(1);

                $("#NormalBUFF").val(guidd1.toUpperCase());
                $("#NHISBUFF").val(guidd2.toUpperCase());

                
                //alert(guidd);

                if ($("#auth_authorizationcode").val().length == 0) {
                    $("#auth_authorizationcode").val(guidd1.toUpperCase());
                }
              
                    var options6 = {
                        url: function (phrase) {
                            return "../Enrollee/GetEnrolleePolicyNumberName?phrase=" + phrase;
                        },

                        getValue: "Policynumber",

                        template: {
                            type: "description",
                            fields: {
                                description: "Name"
                            }
                        },
                        list: {
                            match: {
                                enabled: true
                            },
                            onSelectItemEvent: function () {


                                var value = $("#auth_policynumber").getSelectedItemData().Policynumber;
                                $("#auth_policynumber").val(value).trigger("change");
                                $("#auth_enrolleeName").val('');
                                $("#auth_company_list").val('-1');
                                $("#auth_company_list").trigger('chosen:updated');
                                //$("#enrolleesex").val('');
                                $("#auth_enrolleeId").val('');
                                $("#auth_Plan").val('');

                            }
                        },

                        theme: "plate-dark"
                    };

                    //$("body").delegate("#enrolleeName", "focusin", function () {
                    //        checkpolicy($("#policynumber").val());
                    //        console.log('focusin');
                    //});
                    $("#auth_policynumber").easyAutocomplete(options6);

                   



               

         

            });
    });
    
      
 
    //load providers
    var jqxhr = $.getJSON("../Provider/GetProviderNamesJson", function (data) {

        $("#auth_provider_list").html('');

            $.each(data, function (i, item) {
                $("#auth_provider_list").append($("<option />").val(item.Id).text(item.Name + "-" + item.Address));

                window.providerLocal.setItem(item.Id.toString(), item.Name + "-" + item.Address).then(function () {
                    //return providerLocal.getItem('key');
                }).then(function (value) {
                    // we got our value
                }).catch(function (err) {
                    // we got an error
                });
            });
        })
        .done(function () {
            console.log("Provider Loaded successfully.");

        })
        .fail(function () {
            console.log("error downloading provider list from server quick effecient");
        })
        .always(function () {
            //console.log( "complete" );

            //do chosen
            $("#auth_provider_list").chosen({
                no_results_text: "Oops ,sorry text not found.",
                width: '100%',
                max_shown_results: 12,
                max_selected_options: 1,
            });
        });


    //load Company
    var jqxhr2 = $.getJSON("../Company/GetJson", function (data) {


        $("#auth_company_list").html('');

        $.each(data.aaData, function (i, item) {
            $("#auth_company_list").append($("<option />").val(item.Id).text(item.Name ));
                window.companyLocal.setItem(item.Id.toString(), item.Name ).then(function () {
                    //return providerLocal.getItem('key');
                }).then(function (value) {
                    // we got our value
                }).catch(function (err) {
                    // we got an error
                });
            });
        })
        .done(function () {
            console.log("Company Loaded successfully.");

        })
        .fail(function () {
            console.log("error downloading company list from server quick effecient");
        })
        .always(function () {
            //console.log( "complete" );
            //do chosen
            $("#auth_company_list").chosen({
                no_results_text: "Oops ,sorry text not found.",
                width: '100%',
                max_shown_results: 12,
                max_selected_options: 1,
            });
        });



    var jqxhr3 = $.getJSON("../Users/GetAlLusers", function (data) {

        $("#auth_User_list").html('');

        $.each(data, function (i, item) {
            $("#auth_User_list").append($("<option />").val(item.Id).text(item.Name));

            //providerLocal.setItem(item.Id.toString(), item.Name).then(function () {
            //return providerLocal.getItem('key');
        });
    
}).done(function () {
        console.log("users loaded successfully.");
    })
    .fail(function () {
        console.log("error downloading users list from server quick effecient");
    })
    .always(function () {
        //console.log( "complete" );
    });


}





function guid(e) {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }


    if (e == 1) {
        return 'NHA-NHIS-' + s4() + s4() + '-' + s4();
    }
    return 'NHA-' + s4() + s4() + '-' + s4();

    // return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
    //s4() + '-' + s4() + s4() + s4();
}
function guidCus(e) {

    function s5() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }
    function s4() {
        return Date.now().toString().substring(8, 13); 
    }


    if (e == 1) {
        return 'NHA-NHIS-' + s5() + '-' + s4() + $('#userinitial').val();
    }
    return 'NHA-' + s5() + '-' + s4()  +$('#userinitial').val();

    // return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
    //s4() + '-' + s4() + s4() + s4();
}

function checkpolicy(value) {
    if (true) {

        if (value.length > 15) {

            //alert('hello');

            $.ajax({
                "type": "GET",
                "url": '../Enrollee/GetEnrolleeDetailsClaim/?policyNumber=' + value,
                "data": '',
                beforeSend: function (xhr) {

                    $("#enrolleeloadingdisplay").removeClass("hidden");


                },
                "success": function (msg) {

                    if (msg.respCode == 0) {

                        $("#auth_enrolleeName").val(msg.EnrolleeName);
                        $("#auth_company_list").val(msg.CompanyId);
                        $("#auth_company_list").trigger('chosen:updated');
                        //$("#enrolleesex").val(msg.EnrolleeGender);
                        $("#auth_enrolleeId").val(msg.Id);
                        $("#auth_Plan").val(msg.EnrolleePlan);


                        if (msg.isexpunged) {
                            $("#auth_expungederror").removeClass("hidden");
                            
                        } else {
                            $("#auth_expungederror").addClass("hidden");
                        }
                        //$("#CompanyID").val(msg.CompanyId);


                    } else {
                        toastr["error"](msg.errorMsg, "Input Error");
                        $("#auth_enrolleeName").val('');
                        $("#auth_company_list").val('-1');
                        $("#auth_company_list").trigger('chosen:updated');
                        //$("#enrolleesex").val('');
                        $("#auth_enrolleeId").val('');
                        $("#auth_Plan").val('');
                        $("#auth_expungederror").addClass("hidden");
                        //$("#CompanyID").val('');
                    }

                    $("#enrolleeloadingdisplay").addClass("hidden");


                },
                error: function (xhr, textStatus, error) {
                    if (typeof console == "object") {
                        $("#enrolleeloadingdisplay").addClass("hidden");

                        console.log(xhr.status + "," + xhr.responseText + "," + textStatus + "," + error);
                    }
                }
            });
        }

    }
}
QuickLoadService();

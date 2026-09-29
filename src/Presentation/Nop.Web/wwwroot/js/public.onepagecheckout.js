/*
** nopCommerce one page checkout
*/


/*
** Single page flow: the steps are not an accordion. Each step stays open once reached,
** a step whose choice is already made (saved address, preselected shipping/payment
** method) saves itself and moves on, and changing a choice re-saves from that step down.
** The sticky "place order" button appears once payment info or confirm is reached.
*/
var Checkout = {
    loadWaiting: false,
    failureUrl: false,
    pending: null,
    autoConfirm: false,
    current: 'billing',

    init: function (failureUrl, summaryUrl, couponUrl) {
        this.loadWaiting = false;
        this.failureUrl = failureUrl;
        this.summaryUrl = summaryUrl;
        this.couponUrl = couponUrl;
    },

    start: function () {
        Checkout.renumber();
        //the address cards drive the (hidden) address select
        $(document).on('change', 'input[name=opc_address_card]', function () {
            $('#billing-address-select').val(this.value).trigger('change');
        });
        $(document).on('change', '#billing-address-select', Checkout.addressChanged);
        //the coupon box in the order details step (_OpcSummary): its buttons are the cart page's
        //submit buttons, but there is no form here - post them instead
        $(document).on('click', '#opc-summary-load .apply-discount-coupon-code-button', function (e) {
            e.preventDefault();
            Checkout.applyCoupon();
        });
        $(document).on('keydown', '#opc-summary-load #discountcouponcode', function (e) {
            if (e.keyCode === 13) {
                e.preventDefault();
                Checkout.applyCoupon();
            }
        });
        $(document).on('click', '#opc-summary-load .remove-discount-button', function (e) {
            e.preventDefault();
            Checkout.coupon('OpcRemoveDiscountCoupon', { discountId: this.name.replace('removediscount-', '') });
        });
        $(document).on('change', '#co-shipping-method-form :input', function () {
            Checkout.queue(function () { ShippingMethod.save(); });
        });
        $(document).on('change', '#co-payment-method-form :input', function () {
            Checkout.queue(function () { PaymentMethod.save(); });
        });

        if (Billing.disableBillingAddressCheckoutStep) {
            $('#opc-billing').hide();
            Billing.save();
        } else {
            Checkout.addressChanged();
        }
    },

    //run now, or right after the request in flight finishes (only the latest call is kept)
    queue: function (fn) {
        if (Checkout.loadWaiting === false) {
            fn();
        } else {
            Checkout.pending = fn;
        }
    },

    addressChanged: function () {
        $('input[name=opc_address_card][value="' + $('#billing-address-select').val() + '"]').prop('checked', true);
        if ($('#billing-address-select').val() > 0) {
            Checkout.queue(function () { Billing.save(); });
        } else {
            //a new address has to be typed in first
            Checkout.gotoSection('billing');
        }
    },

    placeOrder: function () {
        if (Checkout.loadWaiting !== false) return;

        if (Checkout.current === 'payment_info') {
            //save the payment details, then confirm straight away (see gotoSection)
            Checkout.autoConfirm = true;
            PaymentInfo.save();
        } else {
            ConfirmOrder.save();
        }
    },

    ajaxFailure: function () {
        location.href = Checkout.failureUrl;
    },

    _disableEnableAll: function (element, isDisabled) {
        var descendants = element.find('*');
        $(descendants).each(function () {
            if (isDisabled) {
                $(this).prop("disabled", true);
            } else {
                $(this).prop("disabled", false);
            }
        });

        if (isDisabled) {
            element.prop("disabled", true);
        } else {
            $(this).prop("disabled", false);
        }
    },

    setLoadWaiting: function (step, keepDisabled) {
        var container;
        if (step) {
            if (this.loadWaiting) {
                this.setLoadWaiting(false);
            }
            container = $('#' + step + '-buttons-container');
            container.addClass('disabled');
            container.css('opacity', '.5');
            this._disableEnableAll(container, true);
            $('#' + step + '-please-wait').show();
        } else {
            if (this.loadWaiting) {
                container = $('#' + this.loadWaiting + '-buttons-container');
                var isDisabled = keepDisabled ? true : false;
                if (!isDisabled) {
                    container.removeClass('disabled');
                    container.css('opacity', '1');
                }
                this._disableEnableAll(container, isDisabled);
                $('#' + this.loadWaiting + '-please-wait').hide();
            }
        }
        this.loadWaiting = step;
        $('.checkout-page').toggleClass('opc-busy', !!step);

        if (!step) {
            //a "place order" press only carries through its own request chain
            this.autoConfirm = false;
            var next = this.pending;
            this.pending = null;
            if (next) {
                next();
            } else {
                //the chain has settled: totals may have changed
                this.refreshSummary();
            }
        }
    },

    //steps skipped by the server (e.g. the only payment method) leave no gap in the numbering
    renumber: function () {
        $('#checkout-steps > li:visible .step-title .number').each(function (i) { $(this).text(i + 1); });
    },

    refreshSummary: function (done) {
        $.ajax({
            cache: false,
            url: this.summaryUrl,
            type: 'GET',
            success: function (html) {
                $('#opc-summary-load').html(html);
                $('#opc-order-total').text($('#opc-summary-load .order-total .value-summary').first().text().trim());
                if (done) done();
            }
        });
    },

    applyCoupon: function () {
        Checkout.coupon('OpcApplyDiscountCoupon', { discountcouponcode: $('#opc-summary-load #discountcouponcode').val() });
    },

    //apply or remove a coupon, then reload the order details (the totals change) and say how it went
    coupon: function (action, data) {
        if (Checkout.loadWaiting !== false) return;
        addAntiForgeryToken(data);
        $.ajax({
            cache: false,
            url: Checkout.couponUrl + action,
            type: 'POST',
            data: data,
            success: function (response) {
                Checkout.refreshSummary(function () {
                    $('#opc-summary-load .opc-coupon-messages').empty().append($.map(response.messages || [], function (message) {
                        return $('<div>').addClass(response.applied ? 'message-success' : 'message-failure').text(message);
                    }));
                });
            },
            error: Checkout.ajaxFailure
        });
    },

    gotoSection: function (name) {
        var section = $('#opc-' + name);
        section.addClass('allow').show();
        //later steps are stale until the chain reaches them again
        section.nextAll('.tab-section').removeClass('allow done').hide();
        section.removeClass('done').prevAll('.tab-section').addClass('done');
        //confirm order only carries errors, terms of service and captcha; nothing to show, no step
        if (name === 'confirm_order' &&
            !$('#checkout-confirm-order-load').find('.message-error, .min-order-warning, .terms-of-service, .captcha-box, input, iframe').length) {
            section.hide();
        }
        Checkout.renumber();
        Checkout.current = name;

        var canPlaceOrder = name === 'payment_info' || name === 'confirm_order';
        $('#confirm-order-buttons-container').prop('hidden', !canPlaceOrder);
        $('body').toggleClass('tm-buybar-on', canPlaceOrder);

        //continue by itself where the choice is already made
        if (name === 'shipping_method' && $('#co-shipping-method-form input[name=shippingoption]:checked').length) {
            Checkout.queue(function () { ShippingMethod.save(); });
        } else if (name === 'payment_method' && $('#co-payment-method-form input[name=paymentmethod]:checked').length) {
            Checkout.queue(function () { PaymentMethod.save(); });
        } else if (name === 'confirm_order' && Checkout.autoConfirm) {
            Checkout.autoConfirm = false;
            Checkout.queue(function () { ConfirmOrder.save(); });
        }
    },

    setStepResponse: function(response) {
      if (response.update_section) {
        $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
      }
      if (response.allow_sections) {
        response.allow_sections.each(function(e) {
          $('#opc-' + e).addClass('allow');
        });
      }

      //TODO move it to a new method
      if ($("#billing-address-select").length > 0) {
        Billing.newAddress(!$('#billing-address-select').val());
      } else {
        Billing.newAddress(true);
      }

      if ($("#shipping-address-select").length > 0) {
        Shipping.newAddress(response.selected_id == undefined ? $('#shipping-address-select').val() : response.selected_id, $('#billing-address-select').children("option:selected").val());
      }

      if (response.goto_section) {
        Checkout.gotoSection(response.goto_section);
        return true;
      }
      if (response.redirect) {
        location.href = response.redirect;
        return true;
      }
      return false;
    }
};


var Billing = {
  form: false,
  getAddressUrl: '',
  saveUrl: '',
  disableBillingAddressCheckoutStep: false,
  guest: false,
  selectedStateId: 0,  

  init: function (form, getAddressUrl, saveUrl, disableBillingAddressCheckoutStep, guest) {
    this.form = form;    
    this.getAddressUrl = getAddressUrl;
    this.saveUrl = saveUrl;
    this.disableBillingAddressCheckoutStep = disableBillingAddressCheckoutStep;
    this.guest = guest;
  },

  newAddress: function (isNew) {
    $('#save-billing-address-button').hide();
    //a saved address saves itself (Checkout.addressChanged); only a typed one needs "Continue"
    $('.new-address-next-step-button').toggle(isNew);
    //leaving edit mode (see editAddress): the cards come back, its buttons go
    $('.opc-address-cards').show();
    $('#cancel-billing-address-button, #delete-billing-address-button').hide();

    if (isNew) {
      $('#billing-new-address-form').show();
    } else {
      $('#billing-new-address-form').hide();
    }
    $(document).trigger({ type: "onepagecheckout_billing_address_new" });
    Billing.initializeCountrySelect();

    if (isNew)
      Billing.editAddress();
  },

  setDefaultCountry: function (defaultCountry) {
    $('#opc-billing select[data-trigger="country-select"] option[value="' + defaultCountry + '"]').prop('selected', true);
    $('#opc-billing select[data-trigger="country-select"] option:selected').change()
  },
  
  resetSelectedAddress: function() {
    var selectElement = $('#billing-address-select');
    if (selectElement) {
      selectElement.val('');
    }
    $(document).trigger({ type: "onepagecheckout_billing_address_reset" });
  },

  save: function() {
    if (Checkout.loadWaiting !== false) return;

    Checkout.setLoadWaiting('billing');

    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: this.nextStep,
      complete: this.resetLoadWaiting,
      error: Checkout.ajaxFailure
    });
  },

  resetLoadWaiting: function() {
    Checkout.setLoadWaiting(false);
  },

  nextStep: function(response) {
    //ensure that response.wrong_billing_address is set
    //if not set, "true" is the default value
    if (typeof response.wrong_billing_address === 'undefined') {
      response.wrong_billing_address = false;
    }
    if (Billing.disableBillingAddressCheckoutStep) {
      $('#opc-billing').toggle(response.wrong_billing_address);
    }


    if (response.error) {
      if (typeof response.message === 'string') {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    //ponytail: a newly typed address was saved; reload so it appears (preselected) in the
    //address list instead of re-rendering the address step client-side. Costs one page load,
    //once per new address.
    if (!Billing.disableBillingAddressCheckoutStep && !($('#billing-address-select').val() > 0)) {
      location.reload();
      return;
    }

    Checkout.setStepResponse(response);
    Billing.initializeCountrySelect();
  },

  initializeCountrySelect: function() {
    if ($('#opc-billing').has('select[data-trigger="country-select"]')) {
      $('#opc-billing select[data-trigger="country-select"]').countrySelect();
    }
  },

  editAddress: function() {
    Billing.resetBillingForm();

    var prefix = 'BillingNewAddress_';
    var selectedItem = $('#billing-address-select').children("option:selected").val();

    $.ajax({
      cache: false,
      type: "GET",
      url: this.getAddressUrl,
      data: {
        addressId: selectedItem,
      },
      success: function (data, textStatus, jqXHR) {
        $.each(data, function (id, value) {
          if (value === null)
            return;

          if (id.indexOf("CustomAddressAttributes") >= 0 && Array.isArray(value)) {
            $.each(value, function (i, customAttribute) {
              if (customAttribute.DefaultValue) {
                $(`#${customAttribute.ControlId}`).val(
                  customAttribute.DefaultValue
                );
              } else {
                $.each(customAttribute.Values, function (j, attributeValue) {
                  if (attributeValue.IsPreSelected) {
                    $(`#${customAttribute.ControlId}`).val(attributeValue.Id);
                    $(
                      `#${customAttribute.ControlId}_${attributeValue.Id}`
                    ).prop("checked", attributeValue.Id);
                  }
                });
              }
            });

            return;
          }

          var val = $(`#${prefix}${id}`).val(value);
          if (id.indexOf("CountryId") >= 0) {
            val.trigger("change");
          }
          if (id.indexOf("StateProvinceId") >= 0) {
            Billing.setSelectedStateId(value);
          }
        });
      },
      complete: function (jqXHR, textStatus) {
        $("#billing-new-address-form").show();
        if (selectedItem != 0) {
          //edit mode: the form replaces the cards, and nothing below may be ordered until the
          //edit is saved or cancelled
          $('.opc-address-cards').hide();
          $("#save-billing-address-button, #cancel-billing-address-button, #delete-billing-address-button").show();
          $('.new-address-next-step-button').hide();
          Checkout.gotoSection('billing');
        }
      },
      error: Checkout.ajaxFailure,
    });
  },

  saveEditAddress: function(url) {
    var selectedId;
    $.ajax({
      cache: false,
      url: url + '?opc=true',
      data: $(this.form).serialize(),
      type: "POST",
      success: function (response) {
        if (response.error) {
          alert(response.message);
          return false;
        } else {
          selectedId = response.selected_id;
          Checkout.setStepResponse(response);
          Billing.resetBillingForm();
        }        
      },
      complete: function() {
        var selectElement = $('#billing-address-select');
        if (selectElement && selectedId) {
          selectElement.val(selectedId);
          //the edit may change the delivery options, so re-save from the address down
          Checkout.addressChanged();
        }
      },
      error: Checkout.ajaxFailure
    });
  },

  //leave edit mode without saving: back to the cards, and re-save the still selected address so
  //the steps below (hidden while editing) come back
  cancelEdit: function () {
    Billing.resetBillingForm();
    Billing.newAddress(false);
    Checkout.addressChanged();
  },

  deleteAddress: function (url) {
    var selectedAddress = $('#billing-address-select').children("option:selected").val();
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      data: {
        "addressId": selectedAddress,
        "opc": 'true'
      },
      success: function (response) {
        Checkout.setStepResponse(response);
        Checkout.addressChanged();
      },
      error: Checkout.ajaxFailure
    });
  },

  setSelectedStateId: function (id) {
    this.selectedStateId = id;
  },

  resetBillingForm: function() {
    $(':input', '#billing-new-address-form')
      .not(':button, :submit, :reset, :hidden')
      .removeAttr('checked').removeAttr('selected')
    $(':input', '#billing-new-address-form')
      .not(':checkbox, :radio, select')
      .val('');

    $('.address-id', '#billing-new-address-form').val('0');
    $('select option[value="0"]', '#billing-new-address-form').prop('selected', true);
  }
};

var Shipping = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

  newAddress: function (id, billingAddressId) {
    isNew = !id;    
        if (isNew) {
          this.resetSelectedAddress();         
          $('#shipping-new-address-form').show();          
          $('#edit-shipping-address-button').hide();
          $('#delete-shipping-address-button').hide();
        } else {
          $('#shipping-new-address-form').hide();
          if (id == billingAddressId || (id != undefined && billingAddressId == undefined)) {
            $('#edit-shipping-address-button').hide();
            $("#save-shipping-address-button").hide();
            $('#delete-shipping-address-button').hide();            
          } else {
            $("#save-shipping-address-button").hide();
            $('#edit-shipping-address-button').show();
            $('#delete-shipping-address-button').show();
          }
        }
        $(document).trigger({ type: "onepagecheckout_shipping_address_new" });
        Shipping.initializeCountrySelect();
    },

    resetSelectedAddress: function () {
        var selectElement = $('#shipping-address-select');
        if (selectElement) {
            selectElement.val('');
        }
        $(document).trigger({ type: "onepagecheckout_shipping_address_reset" });
  },

    editAddress: function (url) {
      Shipping.resetShippingForm();

      var prefix = 'ShippingNewAddress_';
      var selectedItem = $('#shipping-address-select').children("option:selected").val();
      $.ajax({
        cache: false,
        type: "GET",
        url: url,
        data: {
          addressId: selectedItem,
        },
        success: function (data, textStatus, jqXHR) {
          $.each(data, function (id, value) {
            if (value === null)
              return;

            if (id.indexOf("CustomAddressAttributes") >= 0 && Array.isArray(value)) {
              $.each(value, function (i, customAttribute) {
                if (customAttribute.DefaultValue) {
                  $(`#${customAttribute.ControlId}`).val(
                    customAttribute.DefaultValue
                  );
                } else {
                  $.each(customAttribute.Values, function (j, attributeValue) {
                    if (attributeValue.IsPreSelected) {
                      $(`#${customAttribute.ControlId}`).val(attributeValue.Id);
                      $(
                        `#${customAttribute.ControlId}_${attributeValue.Id}`
                      ).prop("checked", attributeValue.Id);
                    }
                  });
                }
              });

              return;
            }

            var val = $(`#${prefix}${id}`).val(value);
            if (id.indexOf("CountryId") >= 0) {
              val.trigger("change");
            }    
            if (id.indexOf("StateProvinceId") >= 0) {
              Billing.setSelectedStateId(value);
            }
          });
        },
        complete: function (jqXHR, textStatus) {
          $("#shipping-new-address-form").show();
          $("#edit-shipping-address-button").hide();
          $("#delete-shipping-address-button").hide();
          $("#save-shipping-address-button").show();
        },
        error: Checkout.ajaxFailure,
      });
    },

    saveEditAddress: function (url) {
      var selectedId;
      $.ajax({
        cache: false,
        url: url + '?opc=true',
        data: $(this.form).serialize(),
        type: "POST",
        success: function (response) {
          if (response.error) {
            alert(response.message);
            return false;
          } else {
            selectedId = response.selected_id;
            Checkout.setStepResponse(response);
            Shipping.resetShippingForm();
          }
        },
        complete: function () {
          var selectElement = $('#shipping-address-select');
          if (selectElement && selectedId) {
            selectElement.val(selectedId);
          }
        },
        error: Checkout.ajaxFailure
      });
    },

    deleteAddress: function (url) {
      var selectedAddress = $('#shipping-address-select').children("option:selected").val();
      $.ajax({
        cache: false,
        type: "GET",
        url: url,
        data: {
          "addressId": selectedAddress,
          "opc": 'true'
        },
        success: function (response) {
          Checkout.setStepResponse(response);
        },
        error: Checkout.ajaxFailure
      });
    },

    save: function () {
        if (Checkout.loadWaiting !== false) return;

        Checkout.setLoadWaiting('shipping');

        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    },

    initializeCountrySelect: function () {
        if ($('#opc-shipping').has('select[data-trigger="country-select"]')) {
            $('#opc-shipping select[data-trigger="country-select"]').countrySelect();
        }
  },

    resetShippingForm: function () {
      $(':input', '#shipping-new-address-form')
        .not(':button, :submit, :reset, :hidden')
        .removeAttr('checked').removeAttr('selected')
      $(':input', '#shipping-new-address-form')
        .not(':checkbox, :radio, select')
        .val('');

      $('.address-id', '#shipping-new-address-form').val('0');
      $('select option[value="0"]', '#shipping-new-address-form').prop('selected', true);
    }
};



var ShippingMethod = {
    form: false,
    saveUrl: false,
    localized_data: false,

    init: function (form, saveUrl, localized_data) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.localized_data = localized_data;
    },

    validate: function () {
        var methods = document.getElementsByName('shippingoption');
        if (methods.length === 0) {
            alert(this.localized_data.NotAvailableMethodsError);
            return false;
        }

        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert(this.localized_data.SpecifyMethodError);
        return false;
    },

    save: function () {
        if (Checkout.loadWaiting !== false) return;

        if (this.validate()) {
            Checkout.setLoadWaiting('shipping-method');

            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: "POST",
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentMethod = {
    form: false,
    saveUrl: false,
    localized_data: false,

    init: function (form, saveUrl, localized_data) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.localized_data = localized_data;
    },

    toggleUseRewardPoints: function (useRewardPointsInput) {
        if (useRewardPointsInput.checked) {
            $('#payment-method-block').hide();
        }
        else {
            $('#payment-method-block').show();
        }
    },

    validate: function () {
        var methods = document.getElementsByName('paymentmethod');
        if (methods.length === 0) {
            alert(this.localized_data.NotAvailableMethodsError);
            return false;
        }

        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert(this.localized_data.SpecifyMethodError);
        return false;
    },

    save: function () {
        if (Checkout.loadWaiting !== false) return;

        if (this.validate()) {
            Checkout.setLoadWaiting('payment-method');
            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: "POST",
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentInfo = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    save: function () {
        if (Checkout.loadWaiting !== false) return;

        Checkout.setLoadWaiting('payment-info');
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var ConfirmOrder = {
    form: false,    
    saveUrl: false,
    isSuccess: false,
    isCaptchaEnabled: false,
    isReCaptchaV3: false,
    recaptchaPublicKey: "",
    div: false,

  init: function (saveUrl, successUrl, isCaptchaEnabled, isReCaptchaV3, recaptchaPublicKey, div) {
        this.div = div;
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
        this.isCaptchaEnabled = isCaptchaEnabled;
        this.isReCaptchaV3 = isReCaptchaV3;
        this.recaptchaPublicKey = recaptchaPublicKey;
    },

  save: async function () {
    if (Checkout.loadWaiting !== false) return;

      //terms of service
        var termOfServiceOk = true;
        if ($('#termsofservice').length > 0) {
            //terms of service element exists
            if (!$('#termsofservice').is(':checked')) {
                $("#terms-of-service-warning-box").dialog();
                termOfServiceOk = false;
            } else {
                termOfServiceOk = true;
            }
        }
        if (termOfServiceOk) {
            Checkout.setLoadWaiting('confirm-order');
            var postData = {};

            if (ConfirmOrder.isCaptchaEnabled) {
                var captchaTok = await ConfirmOrder.getCaptchaToken('OpcConfirmOrder');
                postData['g-recaptcha-response'] = captchaTok;
            }

            addAntiForgeryToken(postData);
            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: postData,
                type: "POST",
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        } else {
            return false;
        }
    },

    getCaptchaToken: async function (action) {
        var recaptchaToken = ''
        if (ConfirmOrder.isReCaptchaV3) {
            grecaptcha.ready(() => {
                grecaptcha.execute(this.recaptchaPublicKey, { action: action }).then((token) => {
                    recaptchaToken = token;
                });
            });
            while (recaptchaToken == '') {
              await new Promise(t => setTimeout(t, 100));
            }
        } else {
          recaptchaToken = $(this.div).find('.captcha-box textarea[name="g-recaptcha-response"]').val();
        }

        return recaptchaToken;
    },

    resetLoadWaiting: function (transport) {
        Checkout.setLoadWaiting(false, ConfirmOrder.isSuccess);
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        if (response.redirect) {
            ConfirmOrder.isSuccess = true;
            location.href = response.redirect;
            return;
        }
        if (response.success) {
            ConfirmOrder.isSuccess = true;
            window.location = ConfirmOrder.successUrl;
        }

        Checkout.setStepResponse(response);
    }
};

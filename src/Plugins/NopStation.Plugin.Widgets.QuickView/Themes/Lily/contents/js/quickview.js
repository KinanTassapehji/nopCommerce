
var QuickView = {
  loadWaiting: false,
  picturezoomenabled: false,
  productdetailsurl: '',
  modalselector: '',
  modalbuttonselector: '',
  loaderselector: '',
  localized_data: false,

  init: function (picturezoomenabled, productdetailsurl, modalselector, modalbuttonselector, loaderselector, localized_data) {
    this.loadWaiting = false;
    this.picturezoomenabled = picturezoomenabled;
    this.productdetailsurl = productdetailsurl;
    this.modalselector = modalselector;
    this.modalbuttonselector = modalbuttonselector;
    this.loaderselector = loaderselector;
    this.localized_data = localized_data;
  },

  setLoadWaiting: function (display) {
    if (display) {
      $(QuickView.loaderselector).show();
    }
    else {
      $(QuickView.loaderselector).hide();
    }
    this.loadWaiting = display;
  },

  render_quickview_button: function (label) {
    $('.product-item[data-quick-view!="true"]').each(function (i, obj) {
      var productid = $(this).data('productid');
      var html = '<button type="button" class="button-2 quick-view-button lily-primary-color-bg" data-productid="' + productid + '" title="' + label + '"><i class="icon-zoom"></i></button>';
      $(this).children('.picture').children('.buttons-hover').append(html);
      $(this).attr("data-quick-view", true);
    });

    $('.quick-view-button').click(function (event) {
      var productid = $(this).data('productid');
      QuickView.load_product_details(productid);
    })
  },

  load_product_details: function (productid) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);
    $(QuickView.modalselector + ' .qv-details').html('');
    $(QuickView.modalselector).css('display', 'block');

    $(QuickView.modalbuttonselector).magnificPopup({
      items: {
        src: QuickView.modalselector,
        type: 'inline'
      },
      preloader: true
    });
    $(QuickView.modalbuttonselector).click();

    $.ajax({
      cache: false,
      type: 'GET',
      url: QuickView.productdetailsurl + '?productId=' + encodeURIComponent(productid),
      success: this.success_process,
      error: this.ajaxFailure,
      complete: this.resetLoadWaiting
    });
  },

  success_process: function (response) {
    QuickView.setLoadWaiting(false);
    
    var $container = $(QuickView.modalselector + ' .qv-details');
    var tempDiv = $('<div>').html(response.html);
    var scripts = tempDiv.find('script');
    var scriptContents = [];
    
    scripts.each(function() {
      var scriptContent = this.textContent || this.innerText || $(this).html();
      if (scriptContent && scriptContent.trim()) {
        scriptContents.push(scriptContent);
      }
    });
    
    scripts.remove();
    $container.html(tempDiv.html());
    
    setTimeout(function() {
      scriptContents.forEach(function(scriptContent) {
        try {
          var script = document.createElement('script');
          script.type = 'text/javascript';
          script.text = scriptContent;
          document.body.appendChild(script);
          document.body.removeChild(script);
        } catch(e) {
          // Silently handle script execution errors
        }
      });
      
      var repositionZoomWindow = function() {
        try {
          var $bigZoom = $('#cloud-zoom-big');
          if ($bigZoom.length === 0) {
            return;
          }
          
          var $carousel = $(QuickView.modalselector + ' .picture-details-carousel');
          var $activeImg;
          
          if ($carousel.length > 0) {
            $activeImg = $carousel.find('.owl-item.active img.cloud-zoom-image').first();
          }
          
          if (!$activeImg || $activeImg.length === 0) {
            $activeImg = $(QuickView.modalselector + ' img.cloud-zoom-image').first();
          }
          
          if (!$activeImg || $activeImg.length === 0 || !$activeImg.is(':visible')) {
            return;
          }
          
          var imgOffset = $activeImg.offset();
          if (!imgOffset) {
            return;
          }
          
          var imgWidth = $activeImg.outerWidth() || 0;
          var imgHeight = $activeImg.outerHeight() || 0;
          var zoomWidth = parseInt($bigZoom.css('width')) || imgWidth || 400;
          var zoomHeight = parseInt($bigZoom.css('height')) || imgHeight || 400;
          var scrollTop = $(window).scrollTop() || 0;
          var scrollLeft = $(window).scrollLeft() || 0;
          var windowWidth = $(window).width() || 0;
          var windowHeight = $(window).height() || 0;
          
          if (windowWidth === 0 || windowHeight === 0) {
            return;
          }
          
          if ($bigZoom.parent().length > 0 && $bigZoom.parent().prop('tagName') !== 'BODY') {
            $bigZoom.appendTo('body');
          }
          
          var newLeft = imgOffset.left + imgWidth + 20;
          var newTop = imgOffset.top;
          
          if (newLeft + zoomWidth > windowWidth + scrollLeft) {
            newLeft = imgOffset.left - zoomWidth - 20;
            if (newLeft < scrollLeft) {
              newLeft = scrollLeft + 20;
            }
          }
          
          if (newTop + zoomHeight > windowHeight + scrollTop) {
            newTop = windowHeight + scrollTop - zoomHeight - 20;
            if (newTop < scrollTop) {
              newTop = scrollTop + 20;
            }
          }
          
          if (newTop < scrollTop) {
            newTop = scrollTop + 20;
          }
          
          $bigZoom.css({
            'position': 'fixed',
            'top': (newTop - scrollTop) + 'px',
            'left': (newLeft - scrollLeft) + 'px',
            'z-index': '99999',
            'display': 'block',
            'visibility': 'visible',
            'opacity': '1'
          });
        } catch(e) {
          // Silently handle repositioning errors
        }
      };
      
      try {
        var repositionTimeout;
        var observer = new MutationObserver(function(mutations) {
          try {
            mutations.forEach(function(mutation) {
              mutation.addedNodes.forEach(function(node) {
                if (node && node.id === 'cloud-zoom-big') {
                  clearTimeout(repositionTimeout);
                  repositionTimeout = setTimeout(function() {
                    repositionZoomWindow();
                  }, 50);
                }
              });
            });
          } catch(e) {
            // Silently handle MutationObserver errors
          }
        });
        
        observer.observe(document.body, { childList: true, subtree: true });
        
        var mouseRepositionTimeout;
        $(document).on('mouseenter mousemove', QuickView.modalselector + ' .cloud-zoom, ' + QuickView.modalselector + ' .mousetrap', function() {
          clearTimeout(mouseRepositionTimeout);
          mouseRepositionTimeout = setTimeout(function() {
            repositionZoomWindow();
          }, 100);
        });
        
        var $carousel = $(QuickView.modalselector + ' .picture-details-carousel');
        if ($carousel.length > 0) {
          $carousel.on('changed.owl.carousel', function() {
            setTimeout(function() {
              try {
                if (typeof initZoom === 'function') {
                  initZoom();
                }
                setTimeout(function() {
                  repositionZoomWindow();
                }, 200);
              } catch(e) {
                // Silently handle carousel change errors
              }
            }, 100);
          });
        }
        
        var scrollResizeTimeout;
        $(window).on('scroll resize', function() {
          clearTimeout(scrollResizeTimeout);
          scrollResizeTimeout = setTimeout(function() {
            repositionZoomWindow();
          }, 50);
        });
        
        setTimeout(function() {
          repositionZoomWindow();
        }, 300);
      } catch(e) {
        // Silently handle setup errors
      }
    }, 100);

    $(QuickView.modalselector + " .mfp-close").on("click", function () {
      $(QuickView.modalselector).hide();
      $(QuickView.modalselector + ' .qv-details').html('');
    });

    $(QuickView.modalselector + ' .item-grid').addClass("ocarousel owl-carousel");
    $(QuickView.modalselector + ' .item-grid').owlCarousel({
      items: 4,
      margin: 14,
      loop: true,
      autoplay: true,
      autoplayTimeout: 8000,
      nav: true,
      responsive: {
        0: {
          items: 1
        },
        450: {
          items: 2
        },
        768: {
          items: 3
        },
        1200: {
          items: 4
        }
      }
    });
  },

  resetLoadWaiting: function () {
    QuickView.setLoadWaiting(false);
  },

  ajaxFailure: function () {
    alert(this.localized_data.QuickViewFailure);
  }
};
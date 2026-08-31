
var AnywereSlider = {
  sliderdetailsurl: '',
  containerselector: '',
  loaderselector: '',
  loadwait: true,
  localized_data: false,

  init: function (sliderdetailsurl, containerselector, loaderselector, localized_data) {
    this.sliderdetailsurl = sliderdetailsurl;
    this.containerselector = containerselector;
    this.loaderselector = loaderselector;
    this.localized_data = localized_data;
    this.loadwait = true;

    AnywereSlider.check_sliders();

    $(window).scroll(function () {
      if (!AnywereSlider.loadwait) {
        AnywereSlider.check_sliders();
      }
    });
  },

  check_sliders: function () {
    $(AnywereSlider.containerselector + '[data-loaded!="true"]').each(function () {
      var elem = $(this);
      if (AnywereSlider.chek_element_on_screen(elem)) {
        if (!elem.data('loading')) {
          elem.attr('data-loading', true);
          var sliderid = elem.data('sliderid');
          AnywereSlider.load_slider_details(sliderid);
        }
      }
    });

    AnywereSlider.loadwait = false;
  },

  chek_element_on_screen: function (elem) {
    var docViewTop = $(window).scrollTop();
    var docViewBottom = docViewTop + $(window).height();

    var elemTop = elem.offset().top;
    var elemBottom = elemTop + elem.height();

    return ((elemBottom <= docViewBottom && elemBottom >= docViewTop) || (elemTop <= docViewBottom && elemTop >= docViewTop));
  },

  load_slider_details: function (sliderid) {
    $.ajax({
      cache: false,
      type: 'POST',
      data: { sliderId: sliderid },
      url: AnywereSlider.sliderdetailsurl,
      success: function (response) {
        var currentElem = $(AnywereSlider.containerselector + '[data-sliderid="' + sliderid + '"]');

        if (response.result) {
          currentElem.html(response.html);
          if ($(window).width() > 1600) {
            megaMenuWidth();
          };

          var sliderselectorId = '#slider-' + sliderid;
          if ($(sliderselectorId).parents(".homepage-top-slider").length) {
            setTimeout(function () {
              megaMenuHeight();
            }, 300)
          }

        }
        else {
          currentElem.html(AnywereSlider.localized_data.AnywereSliderFailure);
        }
        currentElem.attr('data-loaded', true);
        currentElem.css("min-height", "auto");
      },
      error: AnywereSlider.ajaxFailure
    });
  },

  ajaxFailure: function () {
    $(AnywereSlider.containerselector).html(AnywereSlider.localized_data.AnywereSliderFailure);
  }
};


// Megamenu height for larger device
function megaMenuHeight() {

  setTimeout(function () {
    var homepage = $('html').hasClass("html-home-page");
    var sliderHeight = $(".homepage-top-slider .slider-container").height();
    var megaNavHeight = $(".mm-navbar-nav").height();

    if (sliderHeight && $(window).width() >= 1283) {
      $(".mm-nav-item .sublist.first-level").css("min-height", (sliderHeight - 3) + "px");
    } else {
      $(".mm-nav-item .sublist.first-level").css("min-height", (megaNavHeight + 15) + "px");
    }

    if ($(window).width() >= 1283) {
      $(".html-home-page .header-menu .mm-navbar-nav").css("height", (sliderHeight) + "px");
      var navbarPadding = $(".header-lower").height();
      $(".mm-navbar.not-mobile").css("padding-top", navbarPadding + 5);
    }

    if (homepage && $(window).width() >= 1283) {
      var nav_items_height = 50;
      $(".not-mobile > .mm-navbar-nav > .mm-nav-item").each(function () {
        nav_items_height = nav_items_height + $(this).height();
        if (nav_items_height > sliderHeight || nav_items_height > 500) {
          $(this).addClass("go_others");
          if ($(this).hasClass("other_nav")) {
            $(".other_nav").removeClass("d-none")
          } else {
            $(this).appendTo(".other_nav .sublist");
            //$(this).addClass("d-inline-block");
          }
        }
      })
    }

  }, 300);
}


function megaMenuWidth() {
  if ($(window).width() > 1600) {
    var extraSpace = ($(window).width() - 1600) / 2;
    var menuWidth = $('.mm-navbar.not-mobile').width();
    var anyWhereSliderWidth = $(window).width() - extraSpace - menuWidth;
    $('.homepage-top-slider .top-slider').css('max-width', anyWhereSliderWidth);
    var containerwidth = $('.homepage-top-slider').width();
    var topsliderwidth = containerwidth - menuWidth - 30;
    $('.homepage-top-slider .top-slider').width(topsliderwidth);

  } else {
    $('.homepage-top-slider .top-slider').removeAttr('style')
  }
}


$(document).ready(function () {
  if ($(window).width() > 1600) {
    megaMenuWidth();
  }
});

$(window).on('resize', function () {
  megaMenuHeight();
  if ($(window).width() > 1600) {
    megaMenuWidth();
  }
})

//$(window).scroll(function () {
//  megaMenuHeight();
//});
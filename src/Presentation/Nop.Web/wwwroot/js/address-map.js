//Google map pin picker, shared by the public address form and the admin one.
//init() takes element ids only, so each side keeps its own markup and skin; leave
//cityFieldId/countyFieldId/streetFieldId/zipFieldId out and the pin sets nothing
//but the location field - which is what the admin wants, since staff correcting an
//address should not have it overwritten or locked under them.
window.AddressMap = window.AddressMap || (function () {
    var loading;

    function loadApi(key, language) {
        if (window.google && window.google.maps)
            return Promise.resolve();
        if (!loading) {
            loading = new Promise(function (resolve, reject) {
                window.__addressMapApiReady = resolve;
                var script = document.createElement('script');
                script.async = true;
                script.onerror = reject;
                script.src = 'https://maps.googleapis.com/maps/api/js?key=' + encodeURIComponent(key) +
                    '&language=' + encodeURIComponent(language) + '&callback=__addressMapApiReady';
                document.head.appendChild(script);
            });
        }
        return loading;
    }

    //the location field holds "<address as google names it> (lat,lng)" - the text is what the
    //customer and the courier read, the coordinates at the end are what re-opens the pin.
    //Older addresses hold a bare "lat,lng", which the same pattern still matches.
    var pointPattern = /(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)\)?\s*$/;

    function parsePoint(value) {
        var match = pointPattern.exec(value || '');
        return match ? { lat: parseFloat(match[1]), lng: parseFloat(match[2]) } : null;
    }

    function formatLocation(text, point) {
        var coords = point.lat.toFixed(6) + ',' + point.lng.toFixed(6);
        return text ? text + ' (' + coords + ')' : coords;
    }

    //reverse geocoding an unnamed spot answers with plus codes first ("G6VX+2X، دمشق") - and not
    //only under the plus_code type, google styles nearby premises the same way - then an
    //"Unnamed Road". The first result past those is the one a courier can read; the exact
    //spot is kept by the coordinates anyway.
    var plusCode = /^[2-9CFGHJMPQRVWX]{2,8}\+[2-9CFGHJMPQRVWX]{2,3}\b/;
    function describe(results) {
        for (var i = 0; i < results.length; i++) {
            var text = results[i].formatted_address;
            if (!plusCode.test(text) && text.indexOf('Unnamed Road') !== 0)
                return text;
        }
        return results[0].formatted_address;
    }

    function component(components, type) {
        for (var i = 0; i < components.length; i++)
            if (components[i].types.indexOf(type) >= 0)
                return components[i].long_name;
        return '';
    }

    //reverse geocoding returns several results for one pin, ordered most specific first. The
    //nearest one describes the address, but around here it carries no route for most pins - so
    //for the street, and only for the street, keep looking outwards before giving up.
    function componentAcross(results, type) {
        for (var r = 0; r < results.length; r++) {
            var value = component(results[r].address_components, type);
            if (value)
                return value;
        }
        return '';
    }

    function init(options) {
        var byId = function (id) { return id ? document.getElementById(id) : null; };
        var canvas = byId(options.mapId);
        if (!canvas || canvas.dataset.ready)
            return;
        canvas.dataset.ready = '1';

        var location = byId(options.locationFieldId),
            search = byId(options.searchFieldId),
            locate = byId(options.locateButtonId),
            results = byId(options.resultsId),
            hint = byId(options.hintId);

        //the fields the pin fills - readonly, the map is the only way to set them
        var owned = [byId(options.cityFieldId), byId(options.countyFieldId),
                     byId(options.streetFieldId), byId(options.zipFieldId)];

        function setLocked(locked) {
            owned.forEach(function (field) {
                if (!field)
                    return;
                field.readOnly = locked;
                field.classList.toggle('address-map-owned', locked);
            });
        }

        //the pin is the only way to set the location - typing an address by hand is not an option
        setLocked(true);

        //always assigns, blank included - these fields belong to the pin, so a part the new
        //location has no value for must clear, not keep what the previous pin put there
        function fill(field, value) {
            if (field)
                field.value = value;
        }

        //the pin is the input - no reason to show the raw "lat,lng" textbox as well
        var rawInput = location && location.closest('.inputs, .form-group');
        if (rawInput)
            rawInput.style.display = 'none';

        if (locate && !navigator.geolocation)
            locate.style.display = 'none';

        function degrade() {
            //google unreachable - put the plain "lat,lng" textbox back and hand the fields over,
            //otherwise an outage means nobody can enter an address at all
            var block = canvas.closest('.address-map');
            if (block)
                block.style.display = 'none';
            if (rawInput)
                rawInput.style.display = '';
            setLocked(false);
        }

        //the canvas has to be on screen before the map is built: google sizes the container once,
        //and a display:none one measures zero and then paints grey until something resizes it
        function reveal() {
            canvas.style.display = '';
            if (hint)
                hint.style.display = '';
        }

        var map, marker, geocoder, mapPromise;

        function toPoint(latLng) {
            return { lat: latLng.lat(), lng: latLng.lng() };
        }

        //label is the text of the search result the customer picked; a pin dropped by hand
        //has none and takes whatever the reverse geocode calls the spot
        function drop(point, zoom, label) {
            marker.setPosition(point);
            marker.setVisible(true);
            map.panTo(point);
            if (zoom)
                map.setZoom(zoom);
            function save(text) {
                if (location)
                    location.value = formatLocation(text, point);
                if (search && text)
                    search.value = text;
            }
            //coordinates first, so the pin is kept even if the geocode below never answers
            save(label);
            geocoder.geocode({ location: point }, function (results, status) {
                if (status !== 'OK' || !results.length)
                    return;
                save(label || describe(results));
                var parts = results[0].address_components;
                var county = component(parts, 'neighborhood') || component(parts, 'sublocality_level_1') || component(parts, 'sublocality') || component(parts, 'locality');
                var route = componentAcross(results, 'route');
                // ponytail: google's own placeholder for a road it has no name for - worse than blank in a street field
                if (route === 'Unnamed Road')
                    route = '';
                fill(byId(options.cityFieldId), component(parts, 'locality') || component(parts, 'administrative_area_level_2') || component(parts, 'administrative_area_level_1'));
                fill(byId(options.countyFieldId), county);
                //the street is required and the pin owns it, so the customer cannot type one in -
                //it must never come out blank, even where google has no road name for the pin
                fill(byId(options.streetFieldId), route || componentAcross(results, 'premise') || county);
                fill(byId(options.zipFieldId), component(parts, 'postal_code'));
            });
        }

        function buildMap() {
            var pinned = parsePoint(location && location.value);
            map = new google.maps.Map(canvas, {
                center: pinned || parsePoint(options.defaultCenter) || { lat: 0, lng: 0 },
                zoom: pinned ? 17 : 12,
                mapTypeControl: false,
                streetViewControl: false,
                fullscreenControl: false
            });
            // ponytail: google.maps.Marker is deprecated but still served and needs no cloud-side map id.
            // Move to AdvancedMarkerElement when you create a map id for custom styling anyway.
            marker = new google.maps.Marker({ map: map, position: map.getCenter(), draggable: true, visible: !!pinned });
            geocoder = new google.maps.Geocoder();

            map.addListener('click', function (e) { drop(toPoint(e.latLng)); });
            marker.addListener('dragend', function (e) { drop(toPoint(e.latLng)); });

            //let it settle into its first layout before anything reads getBounds() off it
            return new Promise(function (resolve) {
                google.maps.event.addListenerOnce(map, 'idle', function () { resolve(); });
            });
        }

        //nothing is fetched - and no map load is billed - until the customer asks for the map
        function ensureMap() {
            if (!mapPromise)
                mapPromise = loadApi(options.apiKey, options.language)
                    .then(buildMap)
                    .catch(function (error) { degrade(); throw error; });
            return mapPromise;
        }

        //a rejection here has already been handled by degrade() - swallowing it only keeps the console clean
        function show(then) {
            reveal();
            ensureMap().then(then || function () { }).catch(function () { });
        }

        function clearResults() {
            if (!results)
                return;
            results.textContent = '';
            results.style.display = 'none';
        }

        //google will happily match a street name in the wrong country, and it finds nothing at all
        //for half the syrian addresses - either way the customer has to see what came back
        function listResults(found) {
            if (!results)
                return;
            results.textContent = '';
            if (!found.length) {
                var empty = document.createElement('li');
                empty.className = 'empty';
                empty.textContent = options.noResults;
                results.appendChild(empty);
            }
            found.slice(0, 5).forEach(function (result) {
                var item = document.createElement('li');
                item.textContent = result.formatted_address;
                item.addEventListener('click', function () {
                    clearResults();
                    drop(toPoint(result.geometry.location), 17, result.formatted_address);
                });
                results.appendChild(item);
            });
            results.style.display = '';
        }

        if (search) {
            search.addEventListener('keydown', function (e) {
                if (e.key !== 'Enter')
                    return;
                //the box sits inside the address form - enter must not submit it
                e.preventDefault();
                if (!search.value.trim())
                    return;
                show(function () {
                    //bias to what the customer is looking at, so "المزة" lands in the right city
                    geocoder.geocode({ address: search.value, bounds: map.getBounds() }, function (found, status) {
                        listResults(status === 'OK' && found ? found : []);
                    });
                });
            });
            search.addEventListener('input', function () {
                if (!search.value.trim())
                    clearResults();
            });
        }

        if (locate)
            locate.addEventListener('click', function () {
                navigator.geolocation.getCurrentPosition(function (position) {
                    show(function () {
                        drop({ lat: position.coords.latitude, lng: position.coords.longitude }, 17);
                    });
                }, function () {
                    //refused, or a plain-http origin the browser will not locate on at all - open the
                    //map anyway rather than dead-ending, so the pin can still be dropped by hand
                    show();
                });
            });

        //an address that already carries a pin opens on it
        if (parsePoint(location && location.value))
            show();
    }

    return { init: init };
})();

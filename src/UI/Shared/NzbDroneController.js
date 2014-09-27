﻿'use strict';
define(
    [
        'vent',
        'AppLayout',
        'marionette',
        'Shared/NotFoundView'
    ], function (vent, AppLayout, Marionette, NotFoundView) {
        return Marionette.AppRouter.extend({

            initialize: function () {
                this.listenTo(vent, vent.Events.ServerUpdated, this._onServerUpdated);
            },

            showNotFound: function () {
                this.setTitle('Not Found');
                this.showMainRegion(new NotFoundView(this));
            },

            setTitle: function (title) {
                title = title.toLocaleLowerCase();
                if (title === 'nzbdrone') {
                    document.title = 'nzbdrone';
                }
                else {
                    document.title = title + ' - nzbdrone';
                }
            },

            _onServerUpdated: function () {
                this.pendingUpdate = true;
            },

            showMainRegion: function (view) {
                if (this.pendingUpdate) {
                    window.location.reload();
                }

                else {
                    //AppLayout
                    AppLayout.mainRegion.show(view);
                }
            }
        });
    });


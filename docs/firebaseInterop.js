window.firebaseInterop = {
  initialized: false,
  initError: null,
  db: null,

  initialize: function () {
    return new Promise(function (resolve, reject) {
      if (window.firebaseInterop.initialized) {
        resolve();
        return;
      }

      if (window.firebaseInterop.initError) {
        reject(window.firebaseInterop.initError);
        return;
      }

      if (!window.firebaseConfig) {
        var message = 'Firebase config not found. Copy wwwroot/firebaseConfig.example.js to wwwroot/firebaseConfig.js and add your project settings.';
        window.firebaseInterop.initError = message;
        reject(message);
        return;
      }

      try {
        if (!firebase.apps.length) {
          firebase.initializeApp(window.firebaseConfig);
        }

        var auth = firebase.auth();
        auth.setPersistence(firebase.auth.Auth.Persistence.LOCAL)
          .then(function () {
            // Wait for the first onAuthStateChanged callback so persisted
            // auth state is restored before we signal initialization.
            var unsub = auth.onAuthStateChanged(function (user) {
              try {
                window.firebaseInterop.db = firebase.firestore();
                window.firebaseInterop.initialized = true;
                unsub();
                resolve();
              } catch (err) {
                window.firebaseInterop.initError = err;
                unsub();
                reject(err);
              }
            });
          })
          .catch(function (error) {
            window.firebaseInterop.initError = error;
            reject(error);
          });
      } catch (error) {
        window.firebaseInterop.initError = error;
        reject(error);
      }
    });
  },

  getCurrentUser: async function () {
    await window.firebaseInterop.initialize();
    var user = firebase.auth().currentUser;
    if (!user) {
      return null;
    }

    return {
      uid: user.uid,
      email: user.email || null,
      displayName: user.displayName || null,
      emailVerified: user.emailVerified || false
    };
  },

  signInWithEmailPassword: async function (email, password) {
    await window.firebaseInterop.initialize();
    var userCredential = await firebase.auth().signInWithEmailAndPassword(email, password);
    var user = userCredential.user;
    return {
      uid: user.uid,
      email: user.email || null,
      displayName: user.displayName || null,
      emailVerified: user.emailVerified || false
    };
  },

  signOut: async function () {
    await window.firebaseInterop.initialize();
    await firebase.auth().signOut();
  },

  createUserWithEmailPassword: async function (email, password) {
    await window.firebaseInterop.initialize();
    var userCredential = await firebase.auth().createUserWithEmailAndPassword(email, password);
    var user = userCredential.user;
    return {
      uid: user.uid,
      email: user.email || null,
      displayName: user.displayName || null,
      emailVerified: user.emailVerified || false
    };
  },

  getHandleliste: async function () {
    await window.firebaseInterop.initialize();
    var snapshot = await window.firebaseInterop.db.collection('handleliste')
      .orderBy('pinned', 'desc')
      .orderBy('createdAt', 'desc')
      .get();
    return snapshot.docs.map(function (doc) {
      var data = doc.data();
      return {
        id: doc.id,
        name: data.name || null,
        pinned: data.pinned || false,
        createdAt: data.createdAt ? data.createdAt.toDate().toISOString() : null
      };
    });
  },

  // Realtime subscription support for handleliste collection
  _handlelisteListeners: {},
  subscribeHandleliste: async function (dotNetRef) {
    await window.firebaseInterop.initialize();
    var id = 'sub_' + Date.now() + '_' + Math.floor(Math.random() * 10000);
    var unsub = window.firebaseInterop.db.collection('handleliste')
      .orderBy('pinned', 'desc')
      .orderBy('createdAt', 'desc')
      .onSnapshot({ includeMetadataChanges: false }, function (snapshot) {
        try {
          // Build full items list
          var items = snapshot.docs.map(function (doc) {
            var data = doc.data();
            return {
              id: doc.id,
              name: data.name || null,
              pinned: data.pinned || false,
              createdAt: data.createdAt ? data.createdAt.toDate().toISOString() : null
            };
          });

          // Debug: log change types
          try {
            var changes = snapshot.docChanges();
            changes.forEach(function (c) {
              console.debug('handleliste change:', c.type, c.doc.id, c.doc.data());
            });
          } catch (e) {
            // ignore
          }

          // Invoke .NET callback with full list
          if (dotNetRef && dotNetRef.invokeMethodAsync) {
            dotNetRef.invokeMethodAsync('HandlelisteSnapshot', items).catch(function (err) {
              console.error('Error invoking HandlelisteSnapshot on .NET object', err);
            });
          }
        } catch (e) {
          console.error('Error in handleliste onSnapshot: ', e);
        }
      }, function (error) {
        console.error('handleliste onSnapshot error: ', error);
      });

    window.firebaseInterop._handlelisteListeners[id] = unsub;
    return id;
  },

  unsubscribeHandleliste: async function (id) {
    if (!id) return;
    var unsub = window.firebaseInterop._handlelisteListeners[id];
    if (typeof unsub === 'function') {
      try {
        unsub();
      } catch (e) {
        console.warn('Error calling unsubscribe for handleliste listener', e);
      }
    }
    delete window.firebaseInterop._handlelisteListeners[id];
  },

  addHandlelisteItem: async function (item) {
    await window.firebaseInterop.initialize();

    var now = firebase.firestore.Timestamp.now();
    var docRef = await window.firebaseInterop.db.collection('handleliste').add({
      name: item.name || '',
      pinned: item.pinned || false,
      createdAt: now
    });

    var doc = await docRef.get();
    var data = doc.data();

    return {
      id: doc.id,
      name: data.name || null,
      pinned: data.pinned || false,
      createdAt: data.createdAt ? data.createdAt.toDate().toISOString() : null
    };
  },

  updateHandlelisteItemPinned: async function (id, pinned) {
    await window.firebaseInterop.initialize();
    var docRef = window.firebaseInterop.db.collection('handleliste').doc(id);
    await docRef.update({ pinned: pinned });

    var doc = await docRef.get();
    var data = doc.data();

    return {
      id: doc.id,
      name: data.name || null,
      pinned: data.pinned || false,
      createdAt: data.createdAt ? data.createdAt.toDate().toISOString() : null
    };
  },

  deleteHandlelisteItem: async function (id) {
    await window.firebaseInterop.initialize();
    var docRef = window.firebaseInterop.db.collection('handleliste').doc(id);
    await docRef.delete();
  }
,
  removeAddedClassWhenAnimationEnds: function (itemId) {
    if (!itemId) return;

    var tryFind = function (attemptsLeft) {
      var selector = '[data-item-id="' + itemId + '"]';
      var el = document.querySelector(selector);
      if (!el) {
        if (attemptsLeft > 0) {
          setTimeout(function () { tryFind(attemptsLeft - 1); }, 80);
        }
        return;
      }

      if (!el.classList.contains('added')) {
        // nothing to do
        return;
      }

      var onEnd = function () {
        try { el.classList.remove('added'); } catch (e) { }
        el.removeEventListener('animationend', onEnd);
      };

      el.addEventListener('animationend', onEnd);
    };

    tryFind(6);
  }
};

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

        window.firebaseInterop.db = firebase.firestore();
        window.firebaseInterop.initialized = true;
        resolve();
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
    var snapshot = await window.firebaseInterop.db.collection('handleliste').orderBy('createdAt', 'desc').get();
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
  }
};

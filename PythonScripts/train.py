from __future__ import absolute_import, division, print_function, unicode_literals
import tensorflow as tf
import tensorflow_datasets as tfds
import os
import math
import random
import msgpackrpc
import sys
from tensorflow import keras


IMG_HEIGHT = 224
IMG_WIDTH = 224


class TrainServer(object):
    train_ds=None
    val_ds=None
    model=None
    tensorboard_callback=None
    epoch=0

    def loadDataset(self, path):
        lines_dataset = tf.data.TextLineDataset(path)
        val_size = 12
        self.train_ds = lines_dataset.skip(val_size).map(lambda i: self.process_line(i))
        self.val_ds = lines_dataset.take(val_size).map(lambda i: self.process_line(i))
        self.train_ds = self.configure_for_performance(self.train_ds)
        self.val_ds = self.configure_for_performance(self.val_ds)

    def decode_img(self,img):
        # convert the compressed string to a 3D uint8 tensor
        img = tf.image.decode_jpeg(img, channels=3)
        print("IMG:")
        print(img)
        return tf.image.resize(img, [IMG_WIDTH, IMG_HEIGHT])


    def process_line(self,line):
        parts = tf.strings.split(line, '|')
        label = int(parts[0])
        # load the raw data from the file as a string
        img = tf.io.read_file(parts[1])
        img = self.decode_img(img)
        return img, label

    def dump(self,path):
        obj=self.model.optimizer
        f = open(path.decode(),'w')
        for attr in dir(obj):
          if hasattr( obj, attr ):
           f.write( "obj.%s = %s" % (attr, getattr(obj, attr)))
        f.close()

    def save(self,path):
        #self.model.save(path.decode())
        #tf.saved_model.save(self.model, path.decode())
        keras.models.save_model(self.model,path.decode(),save_format='h5')
        f = open(path.decode()+'.epoch','w')
        f.write('{}'.format(self.epoch))
        f.close()
        print("*** saved model "+path.decode())


        

    def load(self,path):
        #self.model=tf.saved_model.load(path.decode())
        self.model=keras.models.load_model(path.decode())
        #self.model.optimizer=tmp.optimizer;
        f = open(path.decode()+'.epoch', 'r')
        self.epoch = int(f.readline())
        f.close()
        print("*** loaded model "+path.decode())


        
        

    def configure_for_performance(self,ds):
      ds = ds.cache()
      ds = ds.shuffle(buffer_size=1000)
      ds = ds.batch(30)
      ds = ds.prefetch(buffer_size=tf.data.experimental.AUTOTUNE)
      return ds

    def enable_tensorboard(self,folder):
        self.tensorboard_callback=tf.keras.callbacks.TensorBoard(
            log_dir=folder.decode(), histogram_freq=0, batch_size=32, write_graph=True,
            write_grads=False, write_images=False, embeddings_freq=0,
            embeddings_layer_names=None, embeddings_metadata=None, embeddings_data=None,
            update_freq='epoch', profile_batch=2
            )

    def build_model(self):
        num_classes = 7

        self.model = tf.keras.Sequential([
          keras.layers.experimental.preprocessing.Rescaling(1./255),
          keras.layers.Conv2D(32, 3, activation='relu'),
          keras.layers.MaxPooling2D(),
          keras.layers.Conv2D(32, 3, activation='relu'),
          keras.layers.MaxPooling2D(),
          keras.layers.Conv2D(32, 3, activation='relu'),
          keras.layers.MaxPooling2D(),
          keras.layers.Flatten(),
          keras.layers.Dense(128, activation='relu'),
          keras.layers.Dense(num_classes)
        ])

        opt = tf.keras.optimizers.Adam(learning_rate=0.0001)

        self.model.compile(
          optimizer=opt,
          loss=tf.losses.SparseCategoricalCrossentropy(from_logits=True),
          metrics=['sparse_categorical_accuracy'])

        self.model.build((None,IMG_WIDTH,IMG_HEIGHT,3))

    def fit(self):
        cb=None

        if(self.tensorboard_callback!=None):
            cb=[self.tensorboard_callback]

        train_epochs=5

        self.model.fit(
          self.train_ds,
          validation_data=self.val_ds,shuffle=True,
          epochs=self.epoch+train_epochs,callbacks=cb,initial_epoch=self.epoch,use_multiprocessing=True
        )

        self.epoch=self.epoch+train_epochs

    def evaluate(self):
        results = self.model.evaluate(self.val_ds)
        print("test loss, test acc:", results)


    def exit(self):
        sys.exit()

if __name__ == '__main__':
    print("start")
    server = msgpackrpc.Server(TrainServer())
    server.listen(msgpackrpc.Address("localhost", 18801))
    server.start()

import tensorflow as tf
from train import TrainServer


srv= TrainServer()
srv.build_model()
srv.loadDataset(b"C:\\temp\\work\\index.txt")
srv.fit()

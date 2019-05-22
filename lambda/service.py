from __future__ import print_function

import os
import subprocess
import boto3
import base64

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
LIB_DIR = os.path.join(SCRIPT_DIR, 'lib')

def handler(event, context):
    try:
        imgfilepath = '/tmp/imgres.png'
        jsonfilepath = '/tmp/result'
        with open(imgfilepath, "wb") as fh:
            fh.write(base64.decodestring(event['image64']))
        command = 'LD_LIBRARY_PATH={} TESSDATA_PREFIX={} ./tesseract {} {}'.format(
            LIB_DIR,
            SCRIPT_DIR,
            imgfilepath,
            jsonfilepath,
        )
        print(command)

        try:
            print('Start')
            output = subprocess.check_output(command, shell=True, stderr=subprocess.STDOUT)
            print('Finish')
            print(output)
            f = open(jsonfilepath+'.txt')
            result = f.read()
            print(result)
            return result
        except subprocess.CalledProcessError as e:
            print('Error')
            print(e.output)
    except Exception as e:
        print('Error e')
        print(e)
        raise e
    return 0 